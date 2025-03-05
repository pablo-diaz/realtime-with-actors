using System;
using System.Linq;
using System.Threading.Tasks;

using DeviceStateModel.Config;
using DeviceStateApi.Services;

using CSharpFunctionalExtensions;

using Proto;
using MediatR;
using Proto.Context;
using System.Collections.Generic;

namespace DeviceStateModel.Device;

public class DeviceManagerActor: IActor
{
    private readonly PID _watchingZoneManager;
    private readonly IMediator _eventHandler;
    private readonly IQueryServiceForEventStore _queryForEventStore;
    private readonly DeviceMonitoringSetup _setup;

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
        var maybeDevicePidFound = TryFindDevice(context, givenDeviceId);
        return maybeDevicePidFound.HasValue
            ? new (DevicePid: maybeDevicePidFound.Value)
            : new(DevicePid: context.SpawnNamed(
                    props: Props.FromProducer(() => createFn())
                                .WithContextDecorator(defaultContext => {
                                    if(defaultContext is not ActorContext newActorContext) return defaultContext;

                                    var metricTagsSpecification = newActorContext.GetType().GetField("_metricTags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                    var defaultMetricTags = metricTagsSpecification.GetValue(newActorContext) as KeyValuePair<string, object>[];
                                    if(defaultMetricTags is null) return defaultContext;

                                    var newMetricTags = new List<KeyValuePair<string, object>>();
                                    newMetricTags.AddRange(defaultMetricTags);
                                    newMetricTags.Add(new("deviceid", givenDeviceId.Id));

                                    metricTagsSpecification.SetValue(newActorContext, newMetricTags.ToArray());

                                    return newActorContext;
                                }),
                    name: givenDeviceId.Id));
    }

    private static Maybe<PID> TryFindDevice(IContext context, DeviceIdentifier givenDeviceId) =>
        context.System.ProcessRegistry.Find(pattern: givenDeviceId.Id).FirstOrDefault();

    private sealed class AdjustedContextForMetricPurposes : ActorContextDecorator
    {
        public AdjustedContextForMetricPurposes(IContext ctx) : base(ctx) { }


    }

}