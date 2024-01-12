using System.Threading;
using System.Threading.Tasks;

using Domain.Events;

using MediatR;

public sealed class UserNotificationForDeviceLocationHasChangedEvent : INotificationHandler<DeviceLocationHasChanged>
{
    public Task Handle(DeviceLocationHasChanged @event, CancellationToken cancellationToken)
    {
        System.Console.WriteLine($"[UserNotificationForDeviceLocationHasChangedEvent]: new event sent to user about DevId '{@event.DeviceId}'");
        return Task.CompletedTask;
    }
}