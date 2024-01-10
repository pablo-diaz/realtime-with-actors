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
        private readonly IConnection _conn;
        private readonly IModel _channel;
        private readonly RabbitMqConfiguration _config;

        public RabbitMqMessageSender(RabbitMqConfiguration config)
        {
            this._config = config;

            var factory = new ConnectionFactory();
            factory.Uri = new Uri(config.ConnectionString);
            _conn = factory.CreateConnection();

            _channel = _conn.CreateModel();

            _channel.ExchangeDeclare(config.Events.Exchange, ExchangeType.Direct);
            _channel.QueueDeclare(config.Events.Queue, false, false, false, null);
            _channel.QueueBind(config.Events.Queue, config.Events.Exchange, config.Events.RoutingKey, null);
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

            lock(_channel) {
                _channel.BasicPublish(_config.Events.Exchange, _config.Events.RoutingKey, props, System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)));
            }

            System.Console.WriteLine($"Mensaje enviado exitosamente a Rabbit MQ");
            return Task.CompletedTask;
        }
    }
}
