using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

public sealed class PersistenceForDeviceTemperatureHasDecreasedEvent : INotificationHandler<DeviceTemperatureHasDecreased>
{
    private readonly IEventStore _eventStore;

    public PersistenceForDeviceTemperatureHasDecreasedEvent(IEventStore eventStore)
    {
        this._eventStore = eventStore;
    }

    public Task Handle(DeviceTemperatureHasDecreased @event, CancellationToken cancellationToken)
    {
        return _eventStore.StoreTemperatureChangeEvent(new TemperatureChangeEvent(
            DeviceId: @event.DeviceId, LoggedAt: System.DateTimeOffset.UtcNow,
            PreviousTemperature: @event.PreviousTemperature.Value,
            NewTemperature: @event.NewTemperature.Value,
            IsTemperatureIncrease: false,
            Location: (Latitude: @event.WhenDeviceWasLocatedAt.Latitude, Longitude: @event.WhenDeviceWasLocatedAt.Longitude)
        ));
    }
}