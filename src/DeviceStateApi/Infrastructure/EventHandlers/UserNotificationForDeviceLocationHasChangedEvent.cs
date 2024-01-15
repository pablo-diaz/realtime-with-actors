using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

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
            previousLocation: (latitude: @event.PreviousLocation.Latitude, longitude: @event.PreviousLocation.Longitude),
            newLocation: (latitude: @event.NewLocation.Latitude, longitude: @event.NewLocation.Longitude));

        System.Console.WriteLine($"[UserNotificationForDeviceLocationHasChangedEvent]: new event sent to user about DevId '{@event.DeviceId}'");
        return Task.CompletedTask;
    }
}