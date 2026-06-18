using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace EventBus.RabbitMQ
{
    public class EventMapping
    {
        public Type EventType { get; init; }
        public string Exchange { get; init; }
        public string Queue { get; init; }
        public string RoutingKey { get; init; }
    }

    public class RabbitMQOptions
    {
        public string HostName { get; set; }
        public int Port { get; set; } = 5672;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; } = "/";
        public List<EventMapping> EventMappings { get; } = new();
        public RabbitMQOptions Map<TEvent>(string exchange, string queue, string routingKey) 
        {
            EventMappings.Add(new EventMapping
            {
                EventType = typeof(TEvent),
                Exchange = exchange,
                Queue = queue,
                RoutingKey = routingKey
            });
            return this;
        }
    }

    public class RabbitMQ
    {
        private RabbitMQOptions _options { get; set; }
        private IConnection _connection { get; set; }
        public IConnection Connection { get; }
        private IChannel _channel { get; set; }
        public IChannel Channel { get; }

        public List<(string, List<(string, string, string)>)> Channels = new List<(string, List<(string, string, string)>)>()
        {
            ("eventbox.events", new List<(string, string, string)>()
            {
                (nameof(EventCreatedEvent), "eventbox.events.created", "event.create"),
                (nameof(EventUpdatedEvent), "eventbox.events.updated", "event.update"),
                (nameof(EventPublishedEvent), "eventbox.events.published", "event.publish"),
                (nameof(EventUnpublishedEvent), "eventbox.events.unpublished", "event.unpublish"),
                (nameof(TicketTypeCreatedEvent), "eventbox.ticketing.type.created", "ticketing.type.create"),
                (nameof(TicketTypeDeletedEvent), "eventbox.ticketing.type.deleted", "ticketing.type.delete"),
            }),
            ("eventbox.payment", new List<(string, string, string)>()
            {
                (nameof(PaymentConfirmedIntegrationEvent), "eventbox.payment.confirmed", "payment.confirm"),
                (nameof(PaymentFailedIntegrationEvent), "eventbox.payment.failed", "payment.fail"),
            }),
            ("eventbox.ticketing", new List<(string, string, string)>()
            {
                (nameof(TicketCapacityAddedEvent), "eventbox.ticketing.capacity.added", "ticketing.capacity.add")
            })
        };

        public RabbitMQ(RabbitMQOptions options)
        {
            _options = options;
        }

        public async Task Init()
        {
            await ConnectAsync();
            await DeclareAsync();
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

            await _channel.ExchangeDeclareAsync(ExchangeType.Topic, ExchangeType.Topic);
            await _channel.ExchangeDeclareAsync(ExchangeType.Fanout, ExchangeType.Fanout);
            await _channel.ExchangeDeclareAsync(ExchangeType.Direct, ExchangeType.Direct);

            foreach (var (channel, events) in Channels)
            {
                foreach (var (eventName, queue, routingKey) in events)
                {
                    await _channel.QueueDeclareAsync(queue, false, false, false, null);
                    await _channel.QueueBindAsync(queue, ExchangeType.Topic, routingKey, null);
                }
            }
            await _channel.QueueDeclareAsync("eventbox.ticketing.expire.dlx", false, false, false, null);
            await _channel.QueueBindAsync("eventbox.ticketing.expire.dlx", ExchangeType.Direct, "ticketing.expire.*", null);

            await _channel.QueueDeclareAsync("eventbox.ticketing.expire", false, false, false, arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = "eventbox.ticketing.dlx",
                ["x-dead-letter-routing-key"] = "eventbox.ticketing.expire.*"
            });
            await _channel.QueueBindAsync("eventbox.ticketing.expire", ExchangeType.Direct, "ticketing.expire.*", null);
        }
    }

    public class RabbitMQEventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMQOptions _options;
        public RabbitMQ _rabbitMQ { get; set; }
        public RabbitMQEventBus(RabbitMQOptions options, IServiceProvider serviceProvider)
        {
            _options = options;
            _serviceProvider = serviceProvider;
            _rabbitMQ = new RabbitMQ(options);
            _rabbitMQ.Init();
        }
        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent
        {
            await _rabbitMQ.ConnectAsync();
            var attr = _options.EventMappings.FirstOrDefault(m => m.EventType == typeof(TEvent))
                ?? throw new InvalidOperationException($"No mapping registered for {typeof(TEvent).Name}");
            await _rabbitMQ.Channel.BasicPublishAsync(attr.Exchange, attr.RoutingKey, true, body: JsonSerializer.SerializeToUtf8Bytes(@event));
        }

        public async Task SubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
            where TEvent : IIntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>
        {
            await _rabbitMQ.ConnectAsync();
            var consumer = new AsyncEventingBasicConsumer(_rabbitMQ.Channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var @event = JsonSerializer.Deserialize<TEvent>(body);
                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                    await handler.HandleAsync(@event, CancellationToken.None);
                    await _rabbitMQ.Channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    await _rabbitMQ.Channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };
            var attr = _options.EventMappings.FirstOrDefault(m => m.EventType == typeof(TEvent))
                ?? throw new InvalidOperationException($"No mapping registered for {typeof(TEvent).Name}");
            await _rabbitMQ.Channel.BasicConsumeAsync(attr.Queue, autoAck: true, consumer: consumer);
        }
    }
}
