using System.Threading.Tasks;

using DeviceStateModel.Device;

namespace Jobs;

public static class MessageProcessor
{
    public static Task Process(Messages.DeviceEvent message, Infrastructure.ActorSystemConfiguration usingActorSystemConfig)
    {
        usingActorSystemConfig.ActorSystem.Root.Send(usingActorSystemConfig.DeviceManagerActor, Map(from: message));
        return Task.CompletedTask;
    }

    private static TemperatureTraced Map(Messages.DeviceEvent from) =>
        new TemperatureTraced(DeviceId: from.DeviceId, LoggedAt: from.At, Temperature: from.Temperature,
            Coords: new (Latitude: from.Latitude, Longitude: from.Longitude));
}