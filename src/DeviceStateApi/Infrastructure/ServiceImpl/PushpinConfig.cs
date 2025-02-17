namespace Infrastructure;

public sealed class PushpinConfig
{
    public class ServerSentEventConfig
    {
        public string ServiceBaseUrl { get; set; }
    }

    public ServerSentEventConfig SSE { get; set; }
}