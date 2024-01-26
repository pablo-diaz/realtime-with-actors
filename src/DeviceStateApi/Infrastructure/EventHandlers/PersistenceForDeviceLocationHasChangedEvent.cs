using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

public sealed class PersistenceForDeviceLocationHasChangedEvent : INotificationHandler<DeviceLocationHasChanged>
{
    private readonly IEventStore _eventStore;

    public PersistenceForDeviceLocationHasChangedEvent(IEventStore eventStore)
    {
        this._eventStore = eventStore;
    }

    public Task Handle(DeviceLocationHasChanged @event, CancellationToken cancellationToken)
    {
        return _eventStore.StoreLocationChangeEvent(new LocationChangeEvent(
            DeviceId: @event.DeviceId, LoggedAt: System.DateTimeOffset.UtcNow,
            PreviousLocation: (Latitude: @event.PreviousLocation.Latitude, Longitude: @event.PreviousLocation.Longitude),
            NewLocation: (Latitude: @event.NewLocation.Latitude, Longitude: @event.NewLocation.Longitude),
            distanceInKms: @event.PreviousLocation.GetDistanceInKm(to: @event.NewLocation)
        ));
    }
}