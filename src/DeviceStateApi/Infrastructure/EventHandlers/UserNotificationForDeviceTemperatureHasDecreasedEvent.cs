using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

namespace DeviceStateApi.Infrastructure.EventHandlers;

public sealed class UserNotificationForDeviceTemperatureHasDecreasedEvent : INotificationHandler<DeviceTemperatureHasDecreased>
{
    private readonly IUserEventPublisher _publisher;

    public UserNotificationForDeviceTemperatureHasDecreasedEvent(IUserEventPublisher publisher)
    {
        this._publisher = publisher;
    }

    public Task Handle(DeviceTemperatureHasDecreased @event, CancellationToken cancellationToken)
    {
        _publisher.PublishDeviceTemperatureHasDecreasedEvent(forDeviceId: @event.DeviceId, newTemperature: @event.NewTemperature.Value);

        return Task.CompletedTask;
    }
}