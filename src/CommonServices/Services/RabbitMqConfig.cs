namespace Services.Config
{
    public class RabbitMqConfiguration
    {
        public class EventsInfo
        {
            public string Exchange { get; set; }
            public string Queue { get; set; }
            public string RoutingKey { get; set; }
            public int CompetingConsumersCount { get; set; }
        }

        public string ConnectionString { get; set; }
        public EventsInfo Events { get; set; }
    }
}