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

            var watchingZoneManagerProps = Props.FromProducer(() => new WatchingZoneManagerActor());
            var deviceManagerProps = Props.FromProducer(() => new DeviceManagerActor(watchingZoneManager: actorSystem.Root.Spawn(watchingZoneManagerProps)));

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
