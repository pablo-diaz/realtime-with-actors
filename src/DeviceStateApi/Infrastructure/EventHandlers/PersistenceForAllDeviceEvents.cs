using System;
using System.Threading;
using System.Threading.Tasks;

using Domain.Events;
using DeviceStateApi.Services;

using MediatR;

namespace DeviceStateApi.Infrastructure.EventHandlers;

public sealed class PersistenceForAllDeviceEvents : INotificationHandler<DeviceHasBeenCreated>
                                                  , INotificationHandler<DeviceLocationHasChanged>
                                                  , INotificationHandler<DeviceLocationHasChangedToAVeryCloseLocation>
                                                  , INotificationHandler<DeviceTemperatureHasDecreased>
                                                  , INotificationHandler<DeviceTemperatureHasIncreased>
                                                  , INotificationHandler<SimilarDeviceTemperatureWasTraced>  // implementing all these interfaces was a Hack, because it wouldn't work with INotificationHandler<DeviceEvent>
{
    private readonly IEventStore _eventStore;

    public PersistenceForAllDeviceEvents(IEventStore eventStore)
    {
        this._eventStore = eventStore;
    }

    public Task Handle(DeviceEvent @event, CancellationToken cancellationToken) => Store(@event);

    public Task Handle(DeviceHasBeenCreated @event, CancellationToken cancellationToken) => Store(@event);

    public Task Handle(DeviceLocationHasChanged @event, CancellationToken cancellationToken) => Store(@event);

    public Task Handle(DeviceLocationHasChangedToAVeryCloseLocation @event, CancellationToken cancellationToken) => Store(@event);

    public Task Handle(DeviceTemperatureHasDecreased @event, CancellationToken cancellationToken) => Store(@event);

    public Task Handle(DeviceTemperatureHasIncreased @event, CancellationToken cancellationToken) => Store(@event);

    public Task Handle(SimilarDeviceTemperatureWasTraced @event, CancellationToken cancellationToken) => Store(@event);

    private Task Store(DeviceEvent @event) =>
        _eventStore.StoreEvent(@event, at: DateTimeOffset.UtcNow);

}
