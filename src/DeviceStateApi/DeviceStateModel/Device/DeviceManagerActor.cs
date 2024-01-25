using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Proto;
using MediatR;
using DeviceStateServices;
using DeviceStateModel.Config;

namespace DeviceStateModel.Device;

public class DeviceManagerActor: IActor
{
    private readonly PID _watchingZoneManager;
    private readonly IMediator _eventHandler;
    private readonly IEventStore _eventStore;
    private readonly DeviceMonitoringSetup _withSetup;
    private readonly Dictionary<string, PID> _devices = new();

    public DeviceManagerActor(PID watchingZoneManager, IMediator eventHandler, DeviceStateServices.IEventStore eventStore,
        DeviceStateModel.Config.DeviceMonitoringSetup withSetup)
    {
        this._watchingZoneManager = watchingZoneManager;
        this._eventHandler = eventHandler;
        this._eventStore = eventStore;
        this._withSetup = withSetup;
    }

    public Task ReceiveAsync(IContext context) => context.Message switch {
        TemperatureTraced temperatureMessage => Process(context, temperatureMessage),
        _ => Task.CompletedTask
    };

    private Task Process(IContext context, TemperatureTraced message)
    {
        (var devicePid, bool deviceWasJustCreated) = GetOrCreateDevice(context, message.DeviceId,
            () => new DeviceActor(withDeviceId: message.DeviceId, initialLoggedDate: message.LoggedAt,
                                  initialTemperature: message.Temperature, initialCoords: message.Coords,
                                  watchingZoneManager: _watchingZoneManager, eventHandler: _eventHandler,
                                  eventStore: _eventStore, withSetup: _withSetup));

        if(deviceWasJustCreated == false)
            context.Send(devicePid, message);

        return Task.CompletedTask;
    }

    private (PID DevicePid, bool WasItJustCreated) GetOrCreateDevice(IContext context, string givenDeviceId, Func<DeviceActor> createFn)
    {
        if(_devices.ContainsKey(givenDeviceId) == false)
        {
            var props = Props.FromProducer(() => createFn());
            _devices[givenDeviceId] = context.Spawn(props);
            return (DevicePid: _devices[givenDeviceId], WasItJustCreated: true);
        }

        return (DevicePid: _devices[givenDeviceId], WasItJustCreated: false);
    }
}