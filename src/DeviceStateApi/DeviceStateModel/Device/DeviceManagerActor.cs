using System.Threading.Tasks;

using Proto;

namespace DeviceStateModel.Device;

public class DeviceManagerActor: IActor
{
    public Task ReceiveAsync(IContext context) => Task.CompletedTask;
}