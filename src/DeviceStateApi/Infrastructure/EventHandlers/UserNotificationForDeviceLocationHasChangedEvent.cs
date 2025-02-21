using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

namespace DeviceStateApi.Infrastructure.EventHandlers;

public sealed class UserNotificationForDeviceLocationHasChangedEvent : INotificationHandler<DeviceLocationHasChanged>
{
    private readonly IUserEventPublisher _publisher;

    public UserNotificationForDeviceLocationHasChangedEvent(IUserEventPublisher publisher)
    {
        this._publisher = publisher;
    }

    public Task Handle(DeviceLocationHasChanged @event, CancellationToken cancellationToken)
    {
        _publisher.PublishDeviceLocationHasChangedEvent(forDeviceId: @event.DeviceId,
            newLocation: (latitude: @event.NewLocation.Latitude, longitude: @event.NewLocation.Longitude));

        return Task.CompletedTask;
    }
}