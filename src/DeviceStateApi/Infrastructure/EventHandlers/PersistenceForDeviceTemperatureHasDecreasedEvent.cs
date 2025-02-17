using System;
using System.Threading;
using System.Threading.Tasks;

using DeviceStateServices;
using Domain.Events;

using MediatR;

namespace Infrastructure.EventHandlers;

public sealed class PersistenceForDeviceTemperatureHasDecreasedEvent : INotificationHandler<DeviceTemperatureHasDecreased>
{
    private readonly IEventStore _eventStore;

    public PersistenceForDeviceTemperatureHasDecreasedEvent(IEventStore eventStore)
    {
        this._eventStore = eventStore;
    }

    public Task Handle(DeviceTemperatureHasDecreased @event, CancellationToken cancellationToken) =>
        _eventStore.StoreEvent(@event, at: DateTimeOffset.UtcNow);

}