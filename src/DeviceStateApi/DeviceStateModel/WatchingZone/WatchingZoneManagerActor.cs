using System.Threading.Tasks;

using Proto;

namespace DeviceStateModel.WatchingZone;

public class WatchingZoneManagerActor : IActor
{
    public Task ReceiveAsync(IContext context) => Task.CompletedTask;
}