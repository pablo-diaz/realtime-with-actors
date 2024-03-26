using System;
using System.Threading.Tasks;

using Services.Config;

using RabbitMQ.Client;
using System.Text.Json;

namespace Services
{
    // https://www.rabbitmq.com/dotnet-api-guide.html
    public sealed class RabbitMqMessageSender : IMessageSender, IDisposable
    {
        private readonly RabbitMqConfiguration _config;
        private readonly IConnection _conn;
        private readonly IModel _channel;
        private static object syncRoot = new object();

        public RabbitMqMessageSender(RabbitMqConfiguration config)
        {
            this._config = config;

            var factory = new ConnectionFactory();
            factory.Uri = new Uri(config.ConnectionString);
            _conn = factory.CreateConnection();

            _channel = _conn.CreateModel();

            _channel.ExchangeDeclare(exchange: config.Events.Exchange, type: ExchangeType.Direct);
            _channel.QueueDeclare(queue: config.Events.Queue, durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: config.Events.Queue, exchange: config.Events.Exchange, routingKey: config.Events.RoutingKey, arguments: null);
        }

        public void Dispose()
        {
            if(_channel != null && _channel.IsOpen)
                _channel.Dispose();

            if(_conn != null && _conn.IsOpen)
                _conn.Dispose();
        }

        public Task SendMessage(object message)
        {
            var props = _channel.CreateBasicProperties();
            props.ContentType = "application/json";

            lock(syncRoot) {
                _channel.BasicPublish(exchange: _config.Events.Exchange, routingKey: _config.Events.RoutingKey, basicProperties: props,
                    body: System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)));
            }

            return Task.CompletedTask;
        }
    }
}
