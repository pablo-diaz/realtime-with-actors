using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Domain.Common;
using Domain.Events;

using DeviceStateApi.Utils;
using DeviceStateApi.Services;
using DeviceStateModel.Config;

using Proto;
using Proto.Timers;

using MediatR;

namespace DeviceStateModel.Device;

public class DeviceActor: IActor
{
    private Domain.Device _currentState;
    private readonly IReadOnlyList<IDomainEvent> _deviceEventsRaisedWhenCreatingDevice;
    private readonly PID _watchingZoneManager;
    private readonly IMediator _eventHandler;
    private readonly DeviceMonitoringSetup _setup;
    private readonly IQueryServiceForEventStore _queryForEventStore;
    private CancellationTokenSource _mostRecentTokenSetWhenWatchingTimerForDeviceInactivity = null;

    public sealed record InitialCoords(decimal Latitude, decimal Longitude);
    public sealed record InstantiatingParams(string DeviceId, decimal InitialTemperature, InitialCoords InitialCoords,
        PID WatchingZoneManager, IMediator EventHandler, DeviceMonitoringSetup Setup, IQueryServiceForEventStore QueryForEventStore);

    public DeviceActor(InstantiatingParams withParams)
    {
        var initialTemperatureResult = Domain.Temperature.For(value: withParams.InitialTemperature);
        if(initialTemperatureResult.IsFailure)
            LetItCrash(scenario: "Creating device actor", withReason: $"Impossible to initialize Device Actor for DevId '{withParams.DeviceId}', when creating Temperature. Reason: {initialTemperatureResult.Error}");

        var coordsResult = Domain.Coords.For(latitude: withParams.InitialCoords.Latitude, longitude: withParams.InitialCoords.Longitude);
        if(coordsResult.IsFailure)
            LetItCrash(scenario: "Creating device actor", withReason: $"Impossible to initialize Device Actor for DevId '{withParams.DeviceId}', when creating Location coords. Reason: {coordsResult.Error}");

        var commandOutcome = Domain.Device.Create(deviceId: withParams.DeviceId, initialTemperature: initialTemperatureResult.Value, initialCoords: coordsResult.Value);
        if(commandOutcome.IsFailure)
            LetItCrash(scenario: "Creating device actor", withReason: $"Impossible to initialize Device Actor for DevId '{withParams.DeviceId}', when creating device. Reason: {commandOutcome.Error}");

        this._currentState = commandOutcome.Value.AggregateResult;
        this._deviceEventsRaisedWhenCreatingDevice = commandOutcome.Value.DomainEventsRaised;
        this._watchingZoneManager = withParams.WatchingZoneManager;
        this._eventHandler = withParams.EventHandler;
        this._setup = withParams.Setup;
        this._queryForEventStore = withParams.QueryForEventStore;
    }

    public Task ReceiveAsync(IContext context) => context.Message switch {
        Started message =>                                  Handle(context, message),
        TemperatureTraced message =>                        Handle(context, message),
        NoRecentActivityHasBeenTrackedFromDevice message => Handle(context, message),
        _ =>                                                Task.CompletedTask
    };

    private async Task Handle(IContext context, Started message)
    {
        IReadOnlyList<DeviceEvent> eventsToRecoverState = [];
        if(_setup.ShouldTryToLoadActorStateFromEventLogStream)
            eventsToRecoverState = await _queryForEventStore.GetEvents(forDeviceId: _currentState.Id);

        var isThisDeviceBrandNewAndThereIsNoStateToRestore = false == eventsToRecoverState.Any();

        if (isThisDeviceBrandNewAndThereIsNoStateToRestore)
            ProcessAnyDomainEvents(eventsToProcess: _deviceEventsRaisedWhenCreatingDevice, eventPublishedCallback: _ => { });
        else
            RecoverState(fromEvents: eventsToRecoverState);

        GeneralUtils.StartPeriodicTaskToReportMetricAboutCurrentUserMessageCount(ofActor: context,
            withId: _currentState.Id, ActorType: typeof(DeviceActor),
            frequencyReportingThisMetric: TimeSpan.FromSeconds(_setup.FrequencyInSecondsOfReportingMetricAboutInboxLengthOfActors));

        SetWatchingTimerForDeviceInactivity(context);
    }

    private Task Handle(IContext context, TemperatureTraced message)
    {
        var temperatureEvents = HandleTemperatureEvent(newTemperature: message.Temperature);
        var locationEvents = HandleLocationEvent(newLocation: message.Coords);

        ProcessAnyDomainEvents(eventsToProcess: temperatureEvents.Union(locationEvents),
            eventPublishedCallback: @event => NotifyWatchingZoneManagerForLocationChangeEvents(context, @event, when: message.LoggedAt));

        SetWatchingTimerForDeviceInactivity(context);

        return Task.CompletedTask;
    }

    private Task Handle(IContext context, NoRecentActivityHasBeenTrackedFromDevice _)
    {
        Console.WriteLine($"[{_currentState.Id}] No recent activity has been received from device, thus we're shutting it down");
        context.Stop(pid: context.Self);  // calling it synchronously, so that Parent receives a Terminated message for this Child that just stopped. The 'StopAsync' method somehow does not send that Terminated message to the Parent (maybe bug in Proto.Actor??)
        return Task.CompletedTask;
    }

    private IEnumerable<IDomainEvent> HandleTemperatureEvent(decimal newTemperature)
    {
        var newTemperatureResult = Domain.Temperature.For(value: newTemperature);
        if(newTemperatureResult.IsFailure)
            LetItCrash(scenario: "Processing temperature change", withReason: $"Impossible to create temperature for DevId '{_currentState.Id}'. Reason: {newTemperatureResult.Error}");

        var commandOutcome = Domain.Device.ChangeTemperature(to: _currentState, newTemperature: newTemperatureResult.Value, withSimilarityThreshold: _setup.TemperatureSimilarityThreshold);

        _currentState = ApplyEvents(eventsToApply: commandOutcome.DomainEventsRaised.Cast<DeviceEvent>(), toInitialState: _currentState);

        return commandOutcome.DomainEventsRaised;
    }

    private IEnumerable<IDomainEvent> HandleLocationEvent((decimal latitude, decimal longitude) newLocation)
    {
        var newCoordsResult = Domain.Coords.For(latitude: newLocation.latitude, longitude: newLocation.longitude);
        if(newCoordsResult.IsFailure)
            LetItCrash(scenario: "Processing location change event", withReason: $"Impossible to handle change of location for DevId '{_currentState.Id}'. Reason: {newCoordsResult.Error}");

        var commandOutcome = Domain.Device.ChangeLocation(to: _currentState, newLocation: newCoordsResult.Value, withAtLeastDistanceInKm: _setup.MinMovedToleratedDistanceInKms);

        _currentState = ApplyEvents(eventsToApply: commandOutcome.DomainEventsRaised.Cast<DeviceEvent>(), toInitialState: _currentState);

        return commandOutcome.DomainEventsRaised;
    }

    private void NotifyWatchingZoneManagerForLocationChangeEvents(IContext context, IDomainEvent @event, string when)
    {
        if(@event is not DeviceLocationHasChanged locationInfo)
            return;

        context.Send(_watchingZoneManager, new WatchingZone.DeviceLocationChanged(deviceId: _currentState.Id, when: when,
            toCoords: locationInfo.NewLocation));
    }

    private void ProcessAnyDomainEvents(IEnumerable<IDomainEvent> eventsToProcess, Action<IDomainEvent> eventPublishedCallback)
    {
        if(eventsToProcess.Any() == false)
            return;

        foreach(var @event in eventsToProcess)
        {
            _eventHandler.Publish(@event); // we're on purpose not awaiting, so that each side-effect is done in a fire-and-forget way
            eventPublishedCallback(@event);
        }
    }

    private void RecoverState(IReadOnlyList<DeviceEvent> fromEvents)
    {
        _currentState = ApplyEvents(eventsToApply: fromEvents, toInitialState: _currentState);
    }

    private void LetItCrash(string scenario, string withReason)
    {
        LogIt("Exception occured !! Reason: " + withReason);
        throw new Exceptions.WhileHandlingMessageException(scenario: scenario, reason: withReason);
    }

    private void LogIt(string message)
    {
        Console.WriteLine($"[Device {_currentState.Id}]: {message}");
    }

    private static Domain.Device ApplyEvents(IEnumerable<DeviceEvent> eventsToApply, Domain.Device toInitialState) =>
        eventsToApply.Aggregate(seed: toInitialState,
            func: (mostRecentState, eventToApply) => Domain.Device.Apply(@event: eventToApply, to: mostRecentState));

    private void SetWatchingTimerForDeviceInactivity(IContext context)
    {
        _mostRecentTokenSetWhenWatchingTimerForDeviceInactivity?.Cancel();

        if (false == ShouldSetWatchingTimerForDeviceInactivity()) return;

        _mostRecentTokenSetWhenWatchingTimerForDeviceInactivity = context.Scheduler().SendOnce(
            delay: TimeSpan.FromMinutes(_setup.MinsToShutDownIdleDevice),
            target: context.Self,
            message: new NoRecentActivityHasBeenTrackedFromDevice(DeviceId: _currentState.Id));
    }

    private bool ShouldSetWatchingTimerForDeviceInactivity() => _setup.MinsToShutDownIdleDevice > 0;

}