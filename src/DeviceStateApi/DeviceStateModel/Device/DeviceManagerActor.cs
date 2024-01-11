using System.Threading.Tasks;
using System.Collections.Generic;

using Proto;
using System;

namespace DeviceStateModel.Device;

public class DeviceManagerActor: IActor
{
    private readonly PID _watchingZoneManager;
    private readonly Dictionary<string, PID> _devices = new();

    public DeviceManagerActor(PID watchingZoneManager)
    {
        this._watchingZoneManager = watchingZoneManager;
    }

    public Task ReceiveAsync(IContext context) => context.Message switch {
        TemperatureTraced temperatureMessage => Process(context, temperatureMessage),
        _ => Task.CompletedTask
    };

    private Task Process(IContext context, TemperatureTraced message)
    {
        (var devicePid, bool deviceWasJustCreated) = GetOrCreateDevice(context, message.DeviceId,
            () => new DeviceActor(withDeviceId: message.DeviceId, initialLoggedDate: message.LoggedAt,
                                  initialTemperature: message.Temperature, initialCoords: message.Coords));

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