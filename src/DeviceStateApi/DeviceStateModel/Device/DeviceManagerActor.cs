using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DeviceStateModel.Config;
using DeviceStateApi.Services;

using CSharpFunctionalExtensions;

using Proto;
using Proto.Context;

using MediatR;
using DeviceStateApi.Utils;

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
            ActorType: typeof(DeviceManagerActor));

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

        _childDevices[givenDeviceId] = new(DevicePid: context.SpawnNamed(
            name: givenDeviceId.Id,
            props: Props.FromProducer(() => createFn())
                //.WithContextDecorator(ctx =>
                //    TryHackActorContextToEnhanceItsPrivateMetricTags(
                //        originalContextToHack: ctx, withDeviceId: givenDeviceId.Id)),
            ));

        return _childDevices[givenDeviceId];
    }

    private Maybe<PID> TryFindDevice(IContext context, DeviceIdentifier givenDeviceId)
    {
        //context.System.ProcessRegistry.Find(pattern: givenDeviceId.Id).FirstOrDefault();

        return _childDevices.ContainsKey(givenDeviceId)
            ? _childDevices[givenDeviceId].DevicePid 
            : Maybe<PID>.None;
    }

    /*
    private static IContext TryHackActorContextToEnhanceItsPrivateMetricTags(IContext originalContextToHack, string withDeviceId)
    {
        if (originalContextToHack is not ActorContext newActorContext) return originalContextToHack;

        var metricTagsSpecification = newActorContext.GetType().GetField("_metricTags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (metricTagsSpecification?.GetValue(newActorContext) is not KeyValuePair<string, object>[] defaultMetricTags) return originalContextToHack;

        metricTagsSpecification.SetValue(newActorContext, 
            defaultMetricTags
            .Select(kv => kv.Key != "id" ? kv : new KeyValuePair<string, object>("id", withDeviceId))
            .ToArray());

        return newActorContext;
    }
    */

}
