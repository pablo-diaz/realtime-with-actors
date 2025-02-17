using System;
using System.Linq;
using System.Threading.Tasks;

using Domain.Common;
using Domain.Events;

using DeviceStateServices;
using DeviceStateModel.Config;

using Proto;
using MediatR;

namespace DeviceStateModel.Device;

public class DeviceActor: IActor
{
    private readonly Domain.Device _currentState;
    private readonly PID _watchingZoneManager;
    private readonly IMediator _eventHandler;
    private readonly DeviceMonitoringSetup _setup;

    public sealed record InitialCoords(decimal Latitude, decimal Longitude);
    public sealed record InstantiatingParams(string DeviceId, decimal InitialTemperature, InitialCoords InitialCoords,
        PID WatchingZoneManager, IMediator EventHandler, DeviceMonitoringSetup Setup);

    public DeviceActor(InstantiatingParams withParams)
    {
        var initialTemperatureResult = Domain.Temperature.For(value: withParams.InitialTemperature);
        if(initialTemperatureResult.IsFailure)
            LetItCrash(scenario: "Creating device actor", withReason: $"Impossible to initialize Device Actor for DevId '{withParams.DeviceId}', when creating Temperature. Reason: {initialTemperatureResult.Error}");

        var coordsResult = Domain.Coords.For(latitude: withParams.InitialCoords.Latitude, longitude: withParams.InitialCoords.Longitude);
        if(coordsResult.IsFailure)
            LetItCrash(scenario: "Creating device actor", withReason: $"Impossible to initialize Device Actor for DevId '{withParams.DeviceId}', when creating Location coords. Reason: {coordsResult.Error}");

        var newDeviceResult = Domain.Device.Create(deviceId: withParams.DeviceId, initialTemperature: initialTemperatureResult.Value, initialCoords: coordsResult.Value);
        if(newDeviceResult.IsFailure)
            LetItCrash(scenario: "Creating device actor", withReason: $"Impossible to initialize Device Actor for DevId '{withParams.DeviceId}', when creating device. Reason: {newDeviceResult.Error}");

        this._currentState = newDeviceResult.Value;
        this._watchingZoneManager = withParams.WatchingZoneManager;
        this._eventHandler = withParams.EventHandler;
        this._setup = withParams.Setup;
    }

    public Task ReceiveAsync(IContext context) => context.Message switch {
        Started startMessage => Handle(context, startMessage),
        TemperatureTraced temperatureMessage => Handle(context, temperatureMessage),
        _ => Task.CompletedTask
    };

    private Task Handle(IContext context, Started message)
    {
        ProcessAnyDomainEvents(eventPublishedCallback: _ => { });
        return Task.CompletedTask;
    }

    private Task Handle(IContext context, TemperatureTraced message)
    {
        HandleTemperatureEvent(newTemperature: message.Temperature);
        HandleLocationEvent(context, when: message.LoggedAt, newLocation: message.Coords);

        ProcessAnyDomainEvents(eventPublishedCallback: @event => NotifyWatchingZoneManagerForLocationChangeEvents(context, @event, when: message.LoggedAt));

        return Task.CompletedTask;
    }

    private void HandleTemperatureEvent(decimal newTemperature)
    {
        var newTemperatureResult = Domain.Temperature.For(value: newTemperature);
        if(newTemperatureResult.IsFailure)
            LetItCrash(scenario: "Processing temperature change", withReason: $"Impossible to create temperature for DevId '{_currentState.Id}'. Reason: {newTemperatureResult.Error}");

        _currentState.ChangeTemperature(newTemperature: newTemperatureResult.Value, withSimilarityThreshold: _setup.TemperatureSimilarityThreshold);
    }

    private void HandleLocationEvent(IContext context, string when, (decimal latitude, decimal longitude) newLocation)
    {
        var newCoordsResult = Domain.Coords.For(latitude: newLocation.latitude, longitude: newLocation.longitude);
        if(newCoordsResult.IsFailure)
            LetItCrash(scenario: "Processing location change event", withReason: $"Impossible to handle change of location for DevId '{_currentState.Id}'. Reason: {newCoordsResult.Error}");

        _currentState.ChangeLocation(newLocation: newCoordsResult.Value, withAtLeastDistanceInKm: _setup.MinMovedToleratedDistanceInKms);
    }

    private void NotifyWatchingZoneManagerForLocationChangeEvents(IContext context, IDomainEvent @event, string when)
    {
        if(@event is not DeviceLocationHasChanged locationInfo)
            return;

        context.Send(_watchingZoneManager, new WatchingZone.DeviceLocationChanged(deviceId: _currentState.Id, when: when,
            fromCoords: locationInfo.PreviousLocation, toCoords: locationInfo.NewLocation));
    }

    private void ProcessAnyDomainEvents(Action<IDomainEvent> eventPublishedCallback)
    {
        if(_currentState.DomainEvents.Any() == false)
            return;

        var events = _currentState.DomainEvents.ToArray();
        _currentState.ClearDomainEvents();

        foreach(var @event in events)
        {
            _eventHandler.Publish(@event); // we're not awaiting on purpose, so that each side-effect is done in a fire-and-forget way
            eventPublishedCallback(@event);
        }
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

}