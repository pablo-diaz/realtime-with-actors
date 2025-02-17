using System;
using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

namespace Infrastructure.EventHandlers;

public sealed class PersistenceForDeviceTemperatureHasIncreasedEvent : INotificationHandler<DeviceTemperatureHasIncreased>
{
    private readonly IEventStore _eventStore;

    public PersistenceForDeviceTemperatureHasIncreasedEvent(IEventStore eventStore)
    {
        this._eventStore = eventStore;
    }

    public Task Handle(DeviceTemperatureHasIncreased @event, CancellationToken cancellationToken) =>
        _eventStore.StoreEvent(@event, at: DateTimeOffset.UtcNow);

}