using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

public sealed class UserNotificationForDeviceTemperatureHasIncreasedEvent : INotificationHandler<DeviceTemperatureHasIncreased>
{
    private readonly IUserEventPublisher _publisher;

    public UserNotificationForDeviceTemperatureHasIncreasedEvent(IUserEventPublisher publisher)
    {
        this._publisher = publisher;
    }

    public Task Handle(DeviceTemperatureHasIncreased @event, CancellationToken cancellationToken)
    {
        _publisher.PublishDeviceTemperatureHasIncreasedEvent(forDeviceId: @event.DeviceId, previousTemperature: @event.PreviousTemperature.Value, newTemperature: @event.NewTemperature.Value,
            whileLocatedAt: (latitude: @event.WhenDeviceWasLocatedAt.Latitude, longitude: @event.WhenDeviceWasLocatedAt.Longitude));
            
        System.Console.WriteLine($"[UserNotificationForDeviceTemperatureHasIncreasedEvent]: new event sent to user about DevId '{@event.DeviceId}'");
        return Task.CompletedTask;
    }
}