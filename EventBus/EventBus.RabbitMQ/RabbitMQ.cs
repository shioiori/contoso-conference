using Eventbox.EventBus.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace EventBus.RabbitMQ
{
    public class RabbitMQ
    {
        private readonly RabbitMQOptions _options;
        private IConnection _connection = default!;
        public IConnection Connection => _connection;
        private IChannel _publishChannel = default!;
        public IChannel PublishChannel => _publishChannel;
        private IChannel _consumeChannel = default!;
        public IChannel ConsumeChannel => _consumeChannel;

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
            var factory = new ConnectionFactory
            {
                UserName = _options.Username,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                HostName = _options.Host,
                Port = _options.Port
            };

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(5, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

            await retryPolicy.ExecuteAsync(async () =>
            {
                _connection = await factory.CreateConnectionAsync();
                _publishChannel = await _connection.CreateChannelAsync();
                _consumeChannel = await _connection.CreateChannelAsync();
            });
        }

        public async Task DeclareAsync()
        {
            var allMappings = _options.PublishMappings.Concat(_options.SubscribeMappings);

            foreach (var exchange in allMappings.DistinctBy(m => m.Exchange))
                await _consumeChannel.ExchangeDeclareAsync(exchange.Exchange, exchange.ExchangeType);

            var extraQueueNames = _options.ExtraQueues.Select(q => q.Queue).ToHashSet();

            foreach (var mapping in _options.SubscribeMappings)
            {
                if (!extraQueueNames.Contains(mapping.Queue))
                    await _consumeChannel.QueueDeclareAsync(mapping.Queue, false, false, false, null);

                await _consumeChannel.QueueBindAsync(mapping.Queue, mapping.Exchange, mapping.RoutingKey, null);

                await _consumeChannel.QueueDeclareAsync(
                    mapping.DelayQueue,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object?>
                    {
                        ["x-dead-letter-exchange"] = mapping.Exchange,
                        ["x-dead-letter-routing-key"] = mapping.RoutingKey
                    });

                await _consumeChannel.QueueDeclareAsync(mapping.DeadLetterQueue, false, false, false, null);
            }

            foreach (var extra in _options.ExtraQueues)
            {
                await _consumeChannel.QueueDeclareAsync(
                    extra.Queue, false, false, false,
                    extra.Arguments.Count > 0 ? extra.Arguments : null);
                await _consumeChannel.QueueBindAsync(extra.Queue, extra.Exchange, extra.RoutingKey, null);
            }
        }
    }

    public class RabbitMQEventBus : IEventBus, IDelayedEventScheduler, IHostedService
    {
        private const string RetryCountHeader = "x-retry-count";

        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMQOptions _options;
        public RabbitMQ _rabbitMQ { get; set; }

        public RabbitMQEventBus(IOptions<RabbitMQOptions> options, IServiceProvider serviceProvider)
        {
            _options = options.Value;
            _serviceProvider = serviceProvider;
            _rabbitMQ = new RabbitMQ(_options);
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IIntegrationEvent
        {
            var eventType = @event.GetType();
            var mapping = _options.PublishMappings.FirstOrDefault(m => m.EventType == eventType)
                ?? throw new InvalidOperationException($"No publish mapping registered for {eventType.Name}");

            await _rabbitMQ.PublishChannel.BasicPublishAsync(
                exchange: mapping.Exchange,
                routingKey: mapping.RoutingKey,
                mandatory: true,
                basicProperties: CreateBasicProperties(@event),
                body: JsonSerializer.SerializeToUtf8Bytes(@event),
                cancellationToken: cancellationToken);
        }

        public async Task ScheduleAsync<TEvent>(
            TEvent @event,
            DateTimeOffset deliverAt,
            CancellationToken cancellationToken = default)
            where TEvent : IIntegrationEvent
        {
            var eventType = @event.GetType();
            var mapping = _options.SubscribeMappings.FirstOrDefault(m => m.EventType == eventType)
                ?? throw new InvalidOperationException($"No subscribe mapping registered for scheduled event {eventType.Name}");

            var delay = deliverAt - DateTimeOffset.UtcNow;
            var ttlMilliseconds = Math.Max(0, (long)delay.TotalMilliseconds);
            var props = CreateBasicProperties(@event);
            props.Expiration = ttlMilliseconds.ToString();

            await _rabbitMQ.PublishChannel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: mapping.DelayQueue,
                mandatory: true,
                basicProperties: props,
                body: JsonSerializer.SerializeToUtf8Bytes(@event),
                cancellationToken: cancellationToken);
        }

        public async Task SubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
            where TEvent : IIntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>
        {
            var mapping = _options.SubscribeMappings.FirstOrDefault(m => m.EventType == typeof(TEvent))
                ?? throw new InvalidOperationException($"No subscribe mapping registered for {typeof(TEvent).Name}");

            var consumer = new AsyncEventingBasicConsumer(_rabbitMQ.ConsumeChannel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var @event = JsonSerializer.Deserialize<TEvent>(ea.Body.Span)
                        ?? throw new InvalidOperationException($"Failed to deserialize {typeof(TEvent).Name}");

                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                    await handler.HandleAsync(@event, CancellationToken.None);
                    await _rabbitMQ.ConsumeChannel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch
                {
                    var retryCount = GetRetryCount(ea.BasicProperties);
                    var shouldRetry = retryCount < mapping.MaxRetries;

                    await PublishRawToQueueAsync(
                        shouldRetry ? mapping.DelayQueue : mapping.DeadLetterQueue,
                        ea.Body,
                        ea.BasicProperties,
                        retryCount + 1,
                        shouldRetry ? mapping.RetryDelayMilliseconds : null,
                        cancellationToken);

                    await _rabbitMQ.ConsumeChannel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
            };

            await _rabbitMQ.ConsumeChannel.BasicConsumeAsync(
                mapping.Queue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _rabbitMQ.Init();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_rabbitMQ.PublishChannel is { IsOpen: true })
                await _rabbitMQ.PublishChannel.CloseAsync(cancellationToken);
            if (_rabbitMQ.ConsumeChannel is { IsOpen: true })
                await _rabbitMQ.ConsumeChannel.CloseAsync(cancellationToken);
            if (_rabbitMQ.Connection is { IsOpen: true })
                await _rabbitMQ.Connection.CloseAsync(cancellationToken);
        }

        private static BasicProperties CreateBasicProperties(IIntegrationEvent @event) => new()
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = @event.IntegrationEventId.ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            Type = @event.EventType,
            Headers = new Dictionary<string, object?>()
        };

        private async Task PublishRawToQueueAsync(
            string queue,
            ReadOnlyMemory<byte> body,
            IReadOnlyBasicProperties sourceProperties,
            int retryCount,
            int? expirationMilliseconds,
            CancellationToken cancellationToken)
        {
            var headers = sourceProperties.Headers is null
                ? new Dictionary<string, object?>()
                : new Dictionary<string, object?>(sourceProperties.Headers);
            headers[RetryCountHeader] = retryCount;

            var props = new BasicProperties
            {
                ContentType = sourceProperties.ContentType,
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = sourceProperties.MessageId,
                Timestamp = sourceProperties.Timestamp,
                Type = sourceProperties.Type,
                Headers = headers,
                Expiration = expirationMilliseconds?.ToString()
            };

            await _rabbitMQ.PublishChannel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queue,
                mandatory: true,
                basicProperties: props,
                body: body,
                cancellationToken: cancellationToken);
        }

        private static int GetRetryCount(IReadOnlyBasicProperties properties)
        {
            if (properties.Headers?.TryGetValue(RetryCountHeader, out var value) != true || value is null)
                return 0;

            return value switch
            {
                int intValue => intValue,
                long longValue => (int)longValue,
                byte[] bytes when int.TryParse(System.Text.Encoding.UTF8.GetString(bytes), out var parsed) => parsed,
                _ => 0
            };
        }
    }
}
