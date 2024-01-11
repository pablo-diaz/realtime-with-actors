using System.Threading.Tasks;

using Proto;

namespace DeviceStateModel.WatchingZone;

public class WatchingZoneManagerActor : IActor
{
    public Task ReceiveAsync(IContext context) => context.Message switch {
        DeviceLocationChanged deviceLocationChangedEvent => Process(context, deviceLocationChangedEvent),
        _ => Task.CompletedTask
    };

    private Task Process(IContext context, DeviceLocationChanged message)
    {
        System.Console.WriteLine($"[WatchingZoneManagerActor]: Device '{message.deviceId}' has changed its location");
        return Task.CompletedTask;
    }
}