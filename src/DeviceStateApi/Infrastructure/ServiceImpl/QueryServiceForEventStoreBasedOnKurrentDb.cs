using System.Threading.Tasks;
using System.Collections.Generic;

using Domain.Events;
using DeviceStateApi.Services;

using EventStore.Client;

using Microsoft.Extensions.Options;

namespace DeviceStateApi.Infrastructure.ServiceImpl;

public sealed class QueryServiceForEventStoreBasedOnKurrentDb : IQueryServiceForEventStore
{
    private readonly EventStoreClient _eventStoreClient;

    public QueryServiceForEventStoreBasedOnKurrentDb(IOptions<KurrentDbConfig> config)
    {
        _eventStoreClient = new EventStoreClient(EventStoreClientSettings.Create(connectionString: config.Value.ConnectionString));
    }

    public void Dispose()
    {
        _eventStoreClient?.Dispose();
    }

    public async Task<IReadOnlyList<DeviceEvent>> GetEvents(string forDeviceId)
    {
        try
        {
            var eventsQueried = new List<DeviceEvent>();

            await foreach(var @event in _eventStoreClient.ReadStreamAsync(streamName: KurrentDbUtils.GetStreamName(forDeviceId),
                                                                          direction: Direction.Forwards, revision: StreamPosition.Start))
            {
                eventsQueried.Add(Map(from: @event));
            }

            return eventsQueried;
        }
        catch (StreamNotFoundException)
        {
            return [];
        }
    }

    private static DeviceEvent Map(ResolvedEvent from) =>
        KurrentDbUtils.DeserializeFromEventStore(
            //serializedData: System.Text.Json.JsonSerializer.Deserialize<string>(utf8Json: from.Event.Data.ToArray()),
            serializedData: System.Text.Encoding.UTF8.GetString(from.Event.Data.ToArray()),
            fromEventType: from.Event.EventType);

}
