using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DeviceStateModel.Config;
using DeviceStateApi.Utils;
using DeviceStateApi.Services;

using CSharpFunctionalExtensions;

using Proto;

using MediatR;

namespace DeviceStateModel.Device;

public class DeviceManagerActor: IActor
{
    private readonly PID _watchingZoneManager;
    private readonly IMediator _eventHandler;
    private readonly IQueryServiceForEventStore _queryForEventStore;
    private readonly DeviceMonitoringSetup _setup;
    private readonly Dictionary<DeviceIdentifier, DeviceReferenceOutcome> _childDevices = new();

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
        Started message =>              Handle(context, message),
        TemperatureTraced message =>    Handle(context, message),
        _ =>                            Task.CompletedTask
    };

    private Task Handle(IContext context, Started message)
    {
        GeneralUtils.StartPeriodicTaskToReportMetricAboutCurrentUserMessageCount(
            ofActor: context, 
            withId: "DeviceManagerActor", 
            ActorType: typeof(DeviceManagerActor),
            frequencyReportingThisMetric: TimeSpan.FromSeconds(_setup.FrequencyInSecondsOfReportingMetricAboutInboxLengthOfActors));

        return Task.CompletedTask;
    }

    private Task Handle(IContext context, TemperatureTraced message)
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
        var maybeDevicePidFound = TryFindDevice(context, givenDeviceId);

        if (maybeDevicePidFound.HasValue) return new(DevicePid: maybeDevicePidFound.Value);

        _childDevices[givenDeviceId] = new(
            DevicePid: context.SpawnNamed(
                name: givenDeviceId.Id,
                props: Props.FromProducer(producer: () => createFn())
        ));

        return _childDevices[givenDeviceId];
    }

    private Maybe<PID> TryFindDevice(IContext context, DeviceIdentifier givenDeviceId)
    {
        /*
        * This did not work, because it was not Performant for the Throughput I needed,
        * as it was becoming a bottleneck for this actor and messages were being stuck
        * in its Inbox queue. I end up creating a Map of the Child Devices which
        * improved the performance a lot, removing the bottleneck
        * 
        * context.System.ProcessRegistry.Find(pattern: givenDeviceId.Id).FirstOrDefault();
        */

        return _childDevices.ContainsKey(givenDeviceId)
            ? _childDevices[givenDeviceId].DevicePid 
            : Maybe<PID>.None;
    }

}
