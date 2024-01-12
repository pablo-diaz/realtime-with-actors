using System.Threading;
using System.Threading.Tasks;

using Domain.Events;

using MediatR;

public sealed class UserNotificationForDeviceTemperatureHasDecreasedEvent : INotificationHandler<DeviceTemperatureHasDecreased>
{
    public Task Handle(DeviceTemperatureHasDecreased @event, CancellationToken cancellationToken)
    {
        System.Console.WriteLine($"[UserNotificationForDeviceTemperatureHasDecreasedEvent]: new event sent to user about DevId '{@event.DeviceId}'");
        return Task.CompletedTask;
    }
}