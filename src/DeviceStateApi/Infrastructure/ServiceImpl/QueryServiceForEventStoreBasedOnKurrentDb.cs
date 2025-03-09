using System.Threading.Tasks;
using System.Collections.Generic;

using Domain.Events;
using DeviceStateApi.Services;

using Grpc.Core;
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

            var result = _eventStoreClient.ReadStreamAsync(streamName: KurrentDbUtils.GetStreamName(forDeviceId),
                                                           direction: Direction.Forwards, revision: StreamPosition.Start);

            if(await result.ReadState == ReadState.StreamNotFound)
            {
                result.ReadState.Dispose(); // Hack: https://github.com/EventStore/EventStore-Client-Dotnet/issues/72#issuecomment-1330294346
                return [];
            }

            await foreach(var @event in result)
            {
                eventsQueried.Add(Map(from: @event));
            }

            return eventsQueried;
        }
        catch (RpcException ex)
        {
            System.Console.Error.WriteLine($"[QueryServiceForEventStoreBasedOnKurrentDb] Whilst getting events stored for device id '{forDeviceId}', the following error ocurred: {ex.Message}");
            throw;
        }
    }

    private static DeviceEvent Map(ResolvedEvent from) =>
        KurrentDbUtils.DeserializeFromEventStore(
            //serializedData: System.Text.Json.JsonSerializer.Deserialize<string>(utf8Json: from.Event.Data.ToArray()),
            serializedData: System.Text.Encoding.UTF8.GetString(from.Event.Data.ToArray()),
            fromEventType: from.Event.EventType);

}
