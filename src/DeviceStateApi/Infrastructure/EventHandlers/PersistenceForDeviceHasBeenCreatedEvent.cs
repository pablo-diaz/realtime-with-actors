using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

public sealed class PersistenceForDeviceHasBeenCreatedEvent : INotificationHandler<DeviceHasBeenCreated>
{
    private readonly IEventStore _eventStore;

    public PersistenceForDeviceHasBeenCreatedEvent(IEventStore eventStore)
    {
        this._eventStore = eventStore;
    }

    public Task Handle(DeviceHasBeenCreated @event, CancellationToken cancellationToken)
    {
        return _eventStore.StoreDeviceCreatedEvent(new DeviceCreatedEvent(
            DeviceId: @event.DeviceId, LoggedAt: System.DateTimeOffset.UtcNow,
            withTemperature: @event.WithTemperature.Value,
            atLocation: (Latitude: @event.AtLocation.Latitude, Longitude: @event.AtLocation.Longitude)
        ));
    }
}