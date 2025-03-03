using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

namespace DeviceStateApi.Infrastructure.EventHandlers;

public sealed class UserNotificationForDeviceTemperatureHasIncreasedEvent : INotificationHandler<DeviceTemperatureHasIncreased>
{
    private readonly IUserEventPublisher _publisher;

    public UserNotificationForDeviceTemperatureHasIncreasedEvent(IUserEventPublisher publisher)
    {
        this._publisher = publisher;
    }

    public Task Handle(DeviceTemperatureHasIncreased @event, CancellationToken cancellationToken)
    {
        _publisher.PublishDeviceTemperatureHasIncreasedEvent(forDeviceId: @event.DeviceId, newTemperature: @event.NewTemperature.Value);
            
        return Task.CompletedTask;
    }
}