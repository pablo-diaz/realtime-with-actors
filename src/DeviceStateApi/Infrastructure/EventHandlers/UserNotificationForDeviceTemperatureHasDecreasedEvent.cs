using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

public sealed class UserNotificationForDeviceTemperatureHasDecreasedEvent : INotificationHandler<DeviceTemperatureHasDecreased>
{
    private readonly IUserEventPublisher _publisher;

    public UserNotificationForDeviceTemperatureHasDecreasedEvent(IUserEventPublisher publisher)
    {
        this._publisher = publisher;
    }

    public Task Handle(DeviceTemperatureHasDecreased @event, CancellationToken cancellationToken)
    {
        _publisher.PublishDeviceTemperatureHasDecreasedEvent(forDeviceId: @event.DeviceId, previousTemperature: @event.PreviousTemperature.Value, newTemperature: @event.NewTemperature.Value,
            whileLocatedAt: (latitude: @event.WhenDeviceWasLocatedAt.Latitude, longitude: @event.WhenDeviceWasLocatedAt.Longitude));

        System.Console.WriteLine($"[UserNotificationForDeviceTemperatureHasDecreasedEvent]: new event sent to user about DevId '{@event.DeviceId}'");
        return Task.CompletedTask;
    }
}