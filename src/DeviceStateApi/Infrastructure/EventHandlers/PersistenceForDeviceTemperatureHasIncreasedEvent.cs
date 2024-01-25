using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

public sealed class PersistenceForDeviceTemperatureHasIncreasedEvent : INotificationHandler<DeviceTemperatureHasIncreased>
{
    private readonly IEventStore _eventStore;

    public PersistenceForDeviceTemperatureHasIncreasedEvent(IEventStore eventStore)
    {
        this._eventStore = eventStore;
    }

    public Task Handle(DeviceTemperatureHasIncreased @event, CancellationToken cancellationToken)
    {
        return _eventStore.StoreTemperatureChangeEvent(new TemperatureChangeEvent(
            DeviceId: @event.DeviceId, LoggedAt: System.DateTimeOffset.UtcNow,
            PreviousTemperature: @event.PreviousTemperature.Value,
            NewTemperature: @event.NewTemperature.Value,
            IsTemperatureIncrease: true,
            Location: (Latitude: @event.WhenDeviceWasLocatedAt.Latitude, Longitude: @event.WhenDeviceWasLocatedAt.Longitude)
        ));
    }
}