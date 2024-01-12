using System;
using System.Linq;
using System.Threading.Tasks;

using Domain.Common;

using Proto;

namespace DeviceStateModel.Device;

public class DeviceActor: IActor
{
    private readonly Domain.Device _currentState;
    private readonly PID _watchingZoneManager;

    public DeviceActor(string withDeviceId, string initialLoggedDate, decimal initialTemperature, (decimal latitude, decimal longitude) initialCoords, PID watchingZoneManager)
    {
        var coordsResult = Domain.Coords.For(latitude: initialCoords.latitude, longitude: initialCoords.longitude);
        if(coordsResult.IsFailure)
            throw new ApplicationException($"Impossible to initialize Device Actor for DevId '{withDeviceId}'. Reason: {coordsResult.Error}");

        var newDeviceResult = Domain.Device.Create(deviceId: withDeviceId, initialTemperature: initialTemperature, initialCoords: coordsResult.Value);
        if(newDeviceResult.IsFailure)
            throw new ApplicationException($"Impossible to initialize Device Actor for DevId '{withDeviceId}'. Reason: {newDeviceResult.Error}");

        this._currentState = newDeviceResult.Value;
        this._watchingZoneManager = watchingZoneManager;
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
            throw new ApplicationException($"Impossible to handle change of temperature for DevId '{_currentState.Id}'. Reason: {result.Error}");
        
        ProcessAnyDomainEvents();
    }

    private void HandleCoordinatesEvent(IContext context, string when, (decimal latitude, decimal longitude) newLocation)
    {
        var newCoordsResult = Domain.Coords.For(latitude: newLocation.latitude, longitude: newLocation.longitude);
        if(newCoordsResult.IsFailure)
            throw new ApplicationException($"Impossible to handle change of location for DevId '{_currentState.Id}'. Reason: {newCoordsResult.Error}");

        _currentState.ChangeLocation(newLocation: newCoordsResult.Value);
        
        ProcessAnyDomainEvents();
    }

    private void PersistEvent(TemperatureTraced @event)
    {
        // TODO
    }

    private void PersistEvent(IDomainEvent @event)
    {
        // TODO
        Log($"Domain event '{@event.GetType()}' raised");
    }

    private void ProcessAnyDomainEvents()
    {
        if(_currentState.DomainEvents.Any() == false)
            return;

        var events = _currentState.DomainEvents.ToArray();
        _currentState.ClearDomainEvents();

        foreach(var @event in events)
            PersistEvent(@event);
    }

    private void Log(string message)
    {
        System.Console.WriteLine($"[Device {_currentState.Id}]: {message}");
    }
}