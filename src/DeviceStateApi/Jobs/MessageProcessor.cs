using System.Threading.Tasks;

namespace Jobs;

public static class MessageProcessor
{
    public static Task Process(Messages.DeviceEvent message, Infrastructure.ActorSystemConfiguration usingActorSystemConfig)
    {
        System.Console.WriteLine($"Nuevo mensaje: DevId: '{message.DeviceId}'");
        return Task.CompletedTask;
    }
}