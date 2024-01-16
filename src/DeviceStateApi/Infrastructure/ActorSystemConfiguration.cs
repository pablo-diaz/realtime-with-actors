using DeviceStateModel.Device;
using DeviceStateModel.WatchingZone;

using Microsoft.Extensions.DependencyInjection;

using Proto;
using Proto.DependencyInjection;

using MediatR;

namespace Infrastructure;

public static class ActorSystemConfigurationExtensions
{
    public static void AddActorSystem(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(provider => {
            var actorSystem = new ActorSystem(ActorSystemConfig.Setup())
                .WithServiceProvider(provider);

            var eventHandler = provider.GetService<IMediator>()!;
            var eventStore = provider.GetService<DeviceStateServices.IEventStore>()!;

            var watchingZoneManagerProps = Props.FromProducer(() => new WatchingZoneManagerActor(eventHandler: eventHandler));
            var deviceManagerProps = Props.FromProducer(() => new DeviceManagerActor(
                watchingZoneManager: actorSystem.Root.Spawn(watchingZoneManagerProps), eventHandler: eventHandler, eventStore: eventStore));

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
