using Eventbox.EventBus.Core.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EventBus.RabbitMQ
{
    public class RabbitMQOptions
    {
        public string HostName { get; set; }
        public int Port { get; set; } = 5672;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; } = "/";
    }

    public class RabbitMQ
    {
        private RabbitMQOptions _options { get; set; }
        private IConnection _connection { get; set; }
        public IConnection Connection { get; }
        private IChannel _channel { get; set; }
        public IChannel Channel { get; }
        public RabbitMQ(RabbitMQOptions options)
        {
            ConnectAsync();
        }

        public async Task ConnectAsync()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = _options.UserName;
            factory.Password = _options.Password;
            factory.VirtualHost = _options.VirtualHost;
            factory.HostName = _options.HostName;
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
        }

        public async Task DeclareAsync()
        {
            await _channel.ExchangeDeclareAsync("eventbox.events", ExchangeType.Topic);
            await _channel.QueueDeclareAsync("eventbox.ticketing.create", false, false, false, null);
            await _channel.QueueBindAsync("eventbox.ticketing.create", "eventbox.events", "event.create", null);
            await _channel.QueueDeclareAsync("eventbox.ticketing.updated", false, false, false, null);
            await _channel.QueueBindAsync("eventbox.ticketing.update", "eventbox.events", "event.update", null);


            await _channel.ExchangeDeclareAsync("eventbox.ticketing", ExchangeType.Topic);
            await _channel.QueueDeclareAsync("ticketing.expire", false, false, false, arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = "eventbox.ticketing.dlx",
                ["x-dead-letter-routing-key"] = "ticketing.expire.*"
            });
            await _channel.QueueBindAsync("ticketing.expire", "eventbox.ticketing", "ticketing.expire.*", null);
        }

        public async Task DeclareDeadLetterQueue()
        {
            await _channel.ExchangeDeclareAsync("eventbox.ticketing.dlx", ExchangeType.Topic);
            await _channel.QueueDeclareAsync("ticketing.expire.dlx", false, false, false, null);
            await _channel.QueueBindAsync("ticketing.expire.dlx", "eventbox.ticketing", "ticketing.expire.*", null);
        }
    }

    public class RabbitMQEventBus : IEventBus
    {
        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent
        {
            var rabbitmq = new RabbitMQ(new RabbitMQOptions()
            {
                HostName = "localhost",
                UserName = "guest",
            };
            await rabbitmq.ConnectAsync();
            await rabbitmq.Channel.BasicPublishAsync($"eventbox.*", @event.EventType, true, body: JsonSerializer.SerializeToUtf8Bytes(@event));
        }

        public async Task SubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
            where TEvent : IIntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>
        {
            var rabbitmq = new RabbitMQ(new RabbitMQOptions()
            {
                HostName = "localhost",
                UserName = "guest",
            };
            await rabbitmq.ConnectAsync();
            var consumer = new AsyncEventingBasicConsumer(rabbitmq.Channel);
            consumer.ReceivedAsync += (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                return Task.CompletedTask;
            };
            await rabbitmq.Channel.BasicConsumeAsync($"eventbox", autoAck: true, consumer: consumer);
        }
    }
}
