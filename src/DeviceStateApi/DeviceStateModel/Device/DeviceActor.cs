using System.Linq;
using System.Threading.Tasks;

using Proto;
using MediatR;

namespace DeviceStateModel.Device;

public class DeviceActor: IActor
{
    private readonly Domain.Device _currentState;
    private readonly PID _watchingZoneManager;
    private readonly IMediator _eventHandler;

    public DeviceActor(string withDeviceId, string initialLoggedDate, decimal initialTemperature, (decimal latitude, decimal longitude) initialCoords,
        PID watchingZoneManager, IMediator eventHandler)
    {
        var coordsResult = Domain.Coords.For(latitude: initialCoords.latitude, longitude: initialCoords.longitude);
        if(coordsResult.IsFailure)
            LetItCrash(scenario: "Creating device actor", withReason: $"Impossible to initialize Device Actor for DevId '{withDeviceId}', when creating Location coords. Reason: {coordsResult.Error}");

        var newDeviceResult = Domain.Device.Create(deviceId: withDeviceId, initialTemperature: initialTemperature, initialCoords: coordsResult.Value);
        if(newDeviceResult.IsFailure)
            LetItCrash(scenario: "Creating device actor", withReason: $"Impossible to initialize Device Actor for DevId '{withDeviceId}', when creating device. Reason: {newDeviceResult.Error}");

        this._currentState = newDeviceResult.Value;
        this._watchingZoneManager = watchingZoneManager;
        this._eventHandler = eventHandler;
    }

    public Task ReceiveAsync(IContext context) => context.Message switch {
        TemperatureTraced temperatureMessage => Handle(context, temperatureMessage),
        _ => Task.CompletedTask
    };

    private Task Handle(IContext context, TemperatureTraced message)
    {
        HandleTemperatureEvent(newTemperature: message.Temperature);
        HandleCoordinatesEvent(context, when: message.LoggedAt, newLocation: message.Coords);
        PersistEvent(@event: message);
        return Task.CompletedTask;
    }

    private void HandleTemperatureEvent(decimal newTemperature)
    {
        var result = _currentState.ChangeTemperature(newTemperature: newTemperature);
        if(result.IsFailure)
            LetItCrash(scenario: "Processing temperature change", withReason: $"Impossible to handle change of temperature for DevId '{_currentState.Id}'. Reason: {result.Error}");
        
        ProcessAnyDomainEvents();
    }

    private void HandleCoordinatesEvent(IContext context, string when, (decimal latitude, decimal longitude) newLocation)
    {
        var newCoordsResult = Domain.Coords.For(latitude: newLocation.latitude, longitude: newLocation.longitude);
        if(newCoordsResult.IsFailure)
            LetItCrash(scenario: "Processing location change event", withReason: $"Impossible to handle change of location for DevId '{_currentState.Id}'. Reason: {newCoordsResult.Error}");

        _currentState.ChangeLocation(newLocation: newCoordsResult.Value);
        
        ProcessAnyDomainEvents();
    }

    private void PersistEvent(TemperatureTraced @event)
    {
        // TODO
    }

    private void ProcessAnyDomainEvents()
    {
        if(_currentState.DomainEvents.Any() == false)
            return;

        var events = _currentState.DomainEvents.ToArray();
        _currentState.ClearDomainEvents();

        foreach(var @event in events)
            _eventHandler.Publish(@event);
    }

    private void LetItCrash(string scenario, string withReason)
    {
        Log("Exception occured !! Reason: " + withReason);
        throw new DeviceStateModel.Exceptions.WhileHandlingMessageException(scenario: scenario, reason: withReason);
    }

    private void Log(string message)
    {
        System.Console.WriteLine($"[Device {_currentState.Id}]: {message}");
    }
}