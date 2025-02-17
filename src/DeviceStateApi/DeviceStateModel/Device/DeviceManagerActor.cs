using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DeviceStateModel.Config;

using Proto;
using MediatR;

namespace DeviceStateModel.Device;

public class DeviceManagerActor: IActor
{
    private readonly PID _watchingZoneManager;
    private readonly IMediator _eventHandler;
    private readonly DeviceMonitoringSetup _setup;
    private readonly Dictionary<string, PID> _devices = new();

    private sealed record DeviceRef(PID DevicePid, bool WasItJustCreated);

    public DeviceManagerActor(PID watchingZoneManager, IMediator eventHandler, DeviceMonitoringSetup withSetup)
    {
        this._watchingZoneManager = watchingZoneManager;
        this._eventHandler = eventHandler;
        this._setup = withSetup;
    }

    public Task ReceiveAsync(IContext context) => context.Message switch {
        TemperatureTraced temperatureMessage => Process(context, temperatureMessage),
        _ => Task.CompletedTask
    };

    private Task Process(IContext context, TemperatureTraced message)
    {
        var deviceRef = GetOrCreateDevice(context, givenDeviceId: message.DeviceId,
            createFn: () => new DeviceActor(new DeviceActor.InstantiatingParams(DeviceId: message.DeviceId,
                                  InitialTemperature: message.Temperature, Setup: _setup,
                                  InitialCoords: new DeviceActor.InitialCoords(Latitude: message.Coords.latitude, Longitude: message.Coords.longitude),
                                  WatchingZoneManager: _watchingZoneManager, EventHandler: _eventHandler)));

        if(false == deviceRef.WasItJustCreated)
            context.Send(deviceRef.DevicePid, message);

        return Task.CompletedTask;
    }

    private DeviceRef GetOrCreateDevice(IContext context, string givenDeviceId, Func<DeviceActor> createFn)
    {
        if(_devices.ContainsKey(givenDeviceId))
            return new (DevicePid: _devices[givenDeviceId], WasItJustCreated: false);

        var props = Props.FromProducer(() => createFn());
        _devices[givenDeviceId] = context.Spawn(props);
        return new (DevicePid: _devices[givenDeviceId], WasItJustCreated: true);
    }

}