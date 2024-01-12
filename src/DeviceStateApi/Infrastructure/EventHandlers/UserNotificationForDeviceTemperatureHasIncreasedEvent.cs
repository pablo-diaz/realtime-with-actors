using System.Threading;
using System.Threading.Tasks;

using Domain.Events;

using MediatR;

public sealed class UserNotificationForDeviceTemperatureHasIncreasedEvent : INotificationHandler<DeviceTemperatureHasIncreased>
{
    public Task Handle(DeviceTemperatureHasIncreased @event, CancellationToken cancellationToken)
    {
        System.Console.WriteLine($"[UserNotificationForDeviceTemperatureHasIncreasedEvent]: new event sent to user about DevId '{@event.DeviceId}'");
        return Task.CompletedTask;
    }
}