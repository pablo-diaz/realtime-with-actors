using System;
using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

namespace Infrastructure.EventHandlers;

public sealed class PersistenceForDeviceLocationHasChangedEvent : INotificationHandler<DeviceLocationHasChanged>
{
    private readonly IEventStore _eventStore;

    public PersistenceForDeviceLocationHasChangedEvent(IEventStore eventStore)
    {
        this._eventStore = eventStore;
    }

    public Task Handle(DeviceLocationHasChanged @event, CancellationToken cancellationToken) =>
        _eventStore.StoreEvent(@event, at: DateTimeOffset.UtcNow);

}