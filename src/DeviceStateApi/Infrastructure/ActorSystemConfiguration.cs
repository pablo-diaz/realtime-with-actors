using DeviceStateApi.Services;
using DeviceStateModel.Device;
using DeviceStateModel.WatchingZone;

using Proto;
using Proto.DependencyInjection;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class ActorSystemConfigurationExtensions
{
    public static void AddActorSystem(this IServiceCollection serviceCollection, DeviceStateModel.Config.DeviceMonitoringSetup withSetup)
    {
        serviceCollection.AddSingleton(provider => {
            var actorSystem = new ActorSystem(ActorSystemConfig
                    .Setup()
                    .WithMetrics())
                .WithServiceProvider(provider);

            var eventHandler = provider.GetService<IMediator>()!;
            var queryForEventStore = provider.GetService<IQueryServiceForEventStore>()!;

            var watchingZoneManagerProps = Props.FromProducer(() => new WatchingZoneManagerActor(eventHandler: eventHandler));
            var deviceManagerProps = Props.FromProducer(() => new DeviceManagerActor(
                watchingZoneManager: actorSystem.Root.Spawn(watchingZoneManagerProps), eventHandler: eventHandler,
                withSetup: withSetup, queryForEventStore: queryForEventStore));

            return new ActorSystemConfiguration(withActorSystem: actorSystem, withDeviceManagerActor: actorSystem.Root.Spawn(deviceManagerProps));
        });
    }
}

public class ActorSystemConfiguration
{
    public ActorSystem ActorSystem { get; }
    public PID DeviceManagerActor { get; }

    public ActorSystemConfiguration(ActorSystem withActorSystem, PID withDeviceManagerActor)
    {
        ActorSystem = withActorSystem;
        DeviceManagerActor = withDeviceManagerActor;
    }
}
