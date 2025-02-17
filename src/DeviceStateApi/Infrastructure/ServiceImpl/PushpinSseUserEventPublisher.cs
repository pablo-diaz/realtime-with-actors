using System.Threading.Tasks;

using DeviceStateServices;

using Flurl;
using Flurl.Http;

namespace Infrastructure;

// Server-sent events publisher using Pushpin  https://pushpin.org/docs/usage/#server-sent-events
public sealed class PushpinSseUserEventPublisher : IUserEventPublisher
{
    private readonly PushpinConfig _configuration;

    public PushpinSseUserEventPublisher(PushpinConfig config)
    {
        this._configuration = config;
    }

    public Task PublishDeviceHasBeenCreatedEvent(string forDeviceId, decimal withTemperature, (decimal latitude, decimal longitude) atLocation)
    {
        return PublishServerSentEvent(ServerSentEventPushpinObject.For(inChannel: DeviceStateConstants.Constants.ChannelNameForGeneralDeviceEventStream,
            eventName: "DeviceHasBeenCreated", eventData: System.Text.Json.JsonSerializer.Serialize(new {
                DevId = forDeviceId,
                Temp = withTemperature,
                AtLoc = new {
                    Lat = atLocation.latitude,
                    Lon = atLocation.longitude
                }
            })));
    }

    public Task PublishDeviceTemperatureHasIncreasedEvent(string forDeviceId, decimal previousTemperature, decimal newTemperature, (decimal latitude, decimal longitude) whileLocatedAt)
    {
        return PublishServerSentEvent(ServerSentEventPushpinObject.For(inChannel: DeviceStateConstants.Constants.ChannelNameForGeneralDeviceEventStream,
            eventName: "DeviceTemperatureHasIncreased", eventData: System.Text.Json.JsonSerializer.Serialize(new {
                DevId = forDeviceId,
                PrevTemp = previousTemperature,
                NewTemp = newTemperature,
                Coords = new {
                    Lat = whileLocatedAt.latitude,
                    Lon = whileLocatedAt.longitude
                }
            })));
    }

    public Task PublishDeviceTemperatureHasDecreasedEvent(string forDeviceId, decimal previousTemperature, decimal newTemperature, (decimal latitude, decimal longitude) whileLocatedAt)
    {
        return PublishServerSentEvent(ServerSentEventPushpinObject.For(inChannel: DeviceStateConstants.Constants.ChannelNameForGeneralDeviceEventStream,
            eventName: "DeviceTemperatureHasDecreased", eventData: System.Text.Json.JsonSerializer.Serialize(new {
                DevId = forDeviceId,
                PrevTemp = previousTemperature,
                NewTemp = newTemperature,
                Coords = new {
                    Lat = whileLocatedAt.latitude,
                    Lon = whileLocatedAt.longitude
                }
            })));
    }

    public Task PublishDeviceLocationHasChangedEvent(string forDeviceId, (decimal latitude, decimal longitude) previousLocation, (decimal latitude, decimal longitude) newLocation)
    {
        return PublishServerSentEvent(ServerSentEventPushpinObject.For(inChannel: DeviceStateConstants.Constants.ChannelNameForGeneralDeviceEventStream,
            eventName: "DeviceLocationHasChanged", eventData: System.Text.Json.JsonSerializer.Serialize(new {
                DeviceId = forDeviceId,
                PrevLoc = new {
                    Lat = previousLocation.latitude,
                    Lon = previousLocation.longitude
                },
                NewLoc = new {
                    Lat = newLocation.latitude,
                    Lon = newLocation.longitude
                }
            })));
    }

    private Task PublishServerSentEvent(ServerSentEventPushpinObject @event)
    {
        _configuration?.SSE?.ServiceBaseUrl
            .AppendPathSegment("publish")
            .PostJsonAsync(@event);

        return Task.CompletedTask;
    }
}

public sealed class ServerSentEventPushpinObject
{
    public class ServerSentEventInfo
    {
        [System.Text.Json.Serialization.JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class ServerSentEventFormat
    {
        [System.Text.Json.Serialization.JsonPropertyName("http-stream")]
        public ServerSentEventInfo Info { get; set; }
    }

    public class ServerSentEventItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("channel")]
        public string Channel { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("formats")]
        public ServerSentEventFormat Formats { get; set; }
    }

    [System.Text.Json.Serialization.JsonPropertyName("items")]
    public ServerSentEventItem[] Items { get; set; }

    public static ServerSentEventPushpinObject For(string inChannel, string eventName, string eventData) =>
        new ServerSentEventPushpinObject {
            Items = [
                new ServerSentEventItem {
                    Channel = inChannel,
                    Formats = new ServerSentEventFormat {
                        Info = new ServerSentEventInfo {
                            Content = $"event: {eventName}\ndata: {eventData}\n\n"
                        }
                    }
                }
            ]
        };
}