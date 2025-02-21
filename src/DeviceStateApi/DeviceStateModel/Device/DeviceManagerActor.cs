using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;

using DeviceStateModel.Config;
using DeviceStateApi.Services;

using Proto;
using MediatR;

namespace DeviceStateModel.Device;

public class DeviceManagerActor: IActor
{
    private readonly PID _watchingZoneManager;
    private readonly IMediator _eventHandler;
    private readonly IQueryServiceForEventStore _queryForEventStore;
    private readonly DeviceMonitoringSetup _setup;
    private readonly Dictionary<DeviceIdentifier, PID> _devices = new();

    public IImmutableSet<PID> Children => throw new NotImplementedException();

    private sealed record DeviceReferenceOutcome(PID DevicePid);

    private sealed record DeviceIdentifier(string Id);

    public DeviceManagerActor(PID watchingZoneManager, IMediator eventHandler, DeviceMonitoringSetup withSetup,
        IQueryServiceForEventStore queryForEventStore)
    {
        this._watchingZoneManager = watchingZoneManager;
        this._eventHandler = eventHandler;
        this._queryForEventStore = queryForEventStore;
        this._setup = withSetup;
    }

    public Task ReceiveAsync(IContext context) => context.Message switch {
        TemperatureTraced temperatureMessage => Process(context, temperatureMessage),
        _ => Task.CompletedTask
    };

    private Task Process(IContext context, TemperatureTraced message)
    {
        var outcome = GetOrCreateDevice(context, givenDeviceId: new(Id: message.DeviceId),
            createFn: () => new DeviceActor(new DeviceActor.InstantiatingParams(DeviceId: message.DeviceId,
                                  InitialTemperature: message.Temperature, Setup: _setup,
                                  InitialCoords: new DeviceActor.InitialCoords(Latitude: message.Coords.latitude, Longitude: message.Coords.longitude),
                                  WatchingZoneManager: _watchingZoneManager, EventHandler: _eventHandler, QueryForEventStore: _queryForEventStore)));

        context.Send(outcome.DevicePid, message);

        return Task.CompletedTask;
    }

    private DeviceReferenceOutcome GetOrCreateDevice(IContext context, DeviceIdentifier givenDeviceId, Func<DeviceActor> createFn)
    {
        if(_devices.ContainsKey(givenDeviceId))
            return new (DevicePid: _devices[givenDeviceId]);

        var props = Props.FromProducer(() => createFn());
        _devices[givenDeviceId] = context.Spawn(props);
        return new (DevicePid: _devices[givenDeviceId]);
    }

}