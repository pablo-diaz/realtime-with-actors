using System.Threading.Tasks;

using Proto;
using MediatR;

namespace DeviceStateModel.WatchingZone;

public class WatchingZoneManagerActor : IActor
{
    private readonly IMediator _eventHandler;

    public WatchingZoneManagerActor(IMediator eventHandler)
    {
        this._eventHandler = eventHandler;
    }

    public Task ReceiveAsync(IContext context) => context.Message switch {
        DeviceLocationChanged deviceLocationChangedEvent => Handle(context, deviceLocationChangedEvent),
        _ => Task.CompletedTask
    };

    private Task Handle(IContext context, DeviceLocationChanged message)
    {
        return Task.CompletedTask;
    }
}