using System;
using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

namespace Infrastructure.EventHandlers;
public sealed class PersistenceForDeviceHasBeenCreatedEvent : INotificationHandler<DeviceHasBeenCreated>
{
    private readonly IEventStore _eventStore;

    public PersistenceForDeviceHasBeenCreatedEvent(IEventStore eventStore)
    {
        this._eventStore = eventStore;
    }

    public Task Handle(DeviceHasBeenCreated @event, CancellationToken cancellationToken) =>
        _eventStore.StoreEvent(@event, at: DateTimeOffset.UtcNow);

}