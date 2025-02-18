using System;
using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

namespace DeviceStateApi.Infrastructure.EventHandlers;

public sealed class PersistenceForAllDeviceEvents : INotificationHandler<DeviceEvent>
{
    private readonly IEventStore _eventStore;

    public PersistenceForAllDeviceEvents(IEventStore eventStore)
    {
        this._eventStore = eventStore;
    }

    public Task Handle(DeviceEvent @event, CancellationToken cancellationToken) =>
        _eventStore.StoreEvent(@event, at: DateTimeOffset.UtcNow);
}
