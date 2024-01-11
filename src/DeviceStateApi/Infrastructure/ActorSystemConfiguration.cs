using Microsoft.Extensions.DependencyInjection;

using Proto;
using Proto.DependencyInjection;

using DeviceStateModel.Device;
using DeviceStateModel.WatchingZone;

namespace Infrastructure;

public static class ActorSystemConfigurationExtensions
{
    public static void AddActorSystem(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(provider => {
            var actorSystem = new ActorSystem(ActorSystemConfig.Setup())
                .WithServiceProvider(provider);

            var deviceManagerProps = Props.FromProducer(() => new DeviceManagerActor());
            var watchingZoneManagerProps = Props.FromProducer(() => new WatchingZoneManagerActor());

            return new ActorSystemConfiguration(withActorSystem: actorSystem,
                withDeviceManagerActor: actorSystem.Root.Spawn(deviceManagerProps),
                withWatchingZoneManagerActor: actorSystem.Root.Spawn(watchingZoneManagerProps));
        });
    }
}

public class ActorSystemConfiguration
{
    public ActorSystem ActorSystem { get; }
    public PID DeviceManagerActor { get; }
    public PID WatchingZoneManagerActor { get; }

    public ActorSystemConfiguration(ActorSystem withActorSystem, PID withDeviceManagerActor, PID withWatchingZoneManagerActor)
    {
        ActorSystem = withActorSystem;
        DeviceManagerActor = withDeviceManagerActor;
        WatchingZoneManagerActor = withWatchingZoneManagerActor;
    }
}
