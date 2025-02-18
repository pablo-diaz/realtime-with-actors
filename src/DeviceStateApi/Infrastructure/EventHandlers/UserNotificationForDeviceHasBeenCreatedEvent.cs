using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

namespace DeviceStateApi.Infrastructure.EventHandlers;

public sealed class UserNotificationForDeviceHasBeenCreatedEvent : INotificationHandler<DeviceHasBeenCreated>
{
    private readonly IUserEventPublisher _publisher;

    public UserNotificationForDeviceHasBeenCreatedEvent(IUserEventPublisher publisher)
    {
        this._publisher = publisher;
    }

    public Task Handle(DeviceHasBeenCreated @event, CancellationToken cancellationToken)
    {
        _publisher.PublishDeviceHasBeenCreatedEvent(forDeviceId: @event.DeviceId,
            withTemperature: @event.WithTemperature.Value,
            atLocation: (latitude: @event.AtLocation.Latitude, longitude: @event.AtLocation.Longitude));

        return Task.CompletedTask;
    }
}