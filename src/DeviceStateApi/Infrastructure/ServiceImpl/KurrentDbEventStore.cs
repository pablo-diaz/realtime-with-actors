using System;
using System.Threading.Tasks;

using Domain.Events;
using DeviceStateApi.Services;

using EventStore.Client;

using Microsoft.Extensions.Options;

namespace DeviceStateApi.Infrastructure.ServiceImpl;

public sealed class KurrentDbEventStore : IEventStore
{
    private readonly EventStoreClient _kurrentDbClient;

    public KurrentDbEventStore(IOptions<KurrentDbConfig> config)
    {
        _kurrentDbClient = new EventStoreClient(EventStoreClientSettings.Create(connectionString: config.Value.ConnectionString));
    }

    public void Dispose()
    {
        _kurrentDbClient?.Dispose();
    }

    public Task StoreEvent(DeviceEvent @event, DateTimeOffset _) 
    {
        return StoreEventInKurrentDb(
            serializedEventData: @event.SerializeToEventStore(),
            toStreamName: KurrentDbUtils.GetStreamName(forDeviceId: @event.DeviceId),
            eventType: @event.GetEventType());
    }

    private Task StoreEventInKurrentDb(string serializedEventData, string toStreamName, string eventType)
    {
        var eventToStore = new EventData(
            eventId: Uuid.NewUuid(),
            type: eventType,
            //data: new ReadOnlyMemory<byte>(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(serializedEventData)),
            data: new ReadOnlyMemory<byte>(System.Text.Encoding.UTF8.GetBytes(serializedEventData)),
            metadata: null,
            contentType: "application/json");

        return _kurrentDbClient.AppendToStreamAsync(
            streamName: toStreamName,
            expectedState: StreamState.Any,
            eventData: [ eventToStore ]);
    }
    
}
