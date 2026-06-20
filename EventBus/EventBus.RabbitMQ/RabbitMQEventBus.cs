using Eventbox.EventBus.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace EventBus.RabbitMQ
{
    public class RabbitMQEventBus : IEventBus, IDelayedEventScheduler, IHostedService
    {
        private const string RetryCountHeader = "x-retry-count";
        private const string OriginalExchangeHeader = "x-original-exchange";
        private const string OriginalRoutingKeyHeader = "x-original-routing-key";

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMQEventBus> _logger;
        private readonly ILogger<RabbitMQClient> _rabbitMqLogger;
        private readonly RabbitMQOptions _options;
        private RabbitMQClient _rabbitMQ { get; set; }
        private SemaphoreSlim _publishLock { get; }

        public RabbitMQEventBus(
            IOptions<RabbitMQOptions> options,
            IServiceProvider serviceProvider,
            ILogger<RabbitMQEventBus> logger,
            ILogger<RabbitMQClient> rabbitMqLogger)
        {
            _options = options.Value;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _rabbitMqLogger = rabbitMqLogger;
            _rabbitMQ = new RabbitMQClient(_options, _rabbitMqLogger);
            _publishLock = new SemaphoreSlim(1, 1);
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IIntegrationEvent
        {
            var eventType = @event.GetType();
            var mapping = _options.PublishMappings.FirstOrDefault(m => m.EventType == eventType)
                ?? throw new InvalidOperationException($"No publish mapping registered for {eventType.Name}");
            await PublishSafeAsync(
                exchange: mapping.Exchange,
                routingKey: mapping.RoutingKey,
                basicProperties: CreateBasicProperties(@event, mapping.Exchange, mapping.RoutingKey),
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
            var props = CreateBasicProperties(@event, mapping.Exchange, mapping.RoutingKey);
            props.Expiration = ttlMilliseconds.ToString();

            await PublishSafeAsync(
                exchange: string.Empty,
                routingKey: mapping.DelayQueue,
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
                catch (Exception ex)
                {
                    var retryCount = GetRetryCount(ea.BasicProperties);
                    var shouldRetry = retryCount < mapping.MaxRetries;
                    var nextQueue = shouldRetry ? mapping.DelayQueue : mapping.DeadLetterQueue;

                    _logger.LogError(
                        ex,
                        "RabbitMQ handler failed for {EventType}. MessageId={MessageId}, Queue={Queue}, RetryCount={RetryCount}, MaxRetries={MaxRetries}, NextQueue={NextQueue}",
                        typeof(TEvent).Name,
                        ea.BasicProperties.MessageId,
                        mapping.Queue,
                        retryCount,
                        mapping.MaxRetries,
                        nextQueue);

                    await PublishRawToQueueAsync(
                        nextQueue,
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

        private static BasicProperties CreateBasicProperties(
            IIntegrationEvent @event,
            string originalExchange,
            string originalRoutingKey) => new()
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = @event.IntegrationEventId.ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Type = @event.EventType,
                Headers = new Dictionary<string, object?>
                {
                    [OriginalExchangeHeader] = originalExchange,
                    [OriginalRoutingKeyHeader] = originalRoutingKey
                }
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

            await PublishSafeAsync(
                exchange: string.Empty,
                routingKey: queue,
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

        private async Task PublishSafeAsync(string exchange,
            string routingKey,
            BasicProperties basicProperties,
            ReadOnlyMemory<byte> body,
            CancellationToken cancellationToken)
        {
            await _publishLock.WaitAsync(cancellationToken);
            try
            {
                await _rabbitMQ.PublishChannel.BasicPublishAsync(
                    exchange: exchange,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: basicProperties,
                    body: body,
                    cancellationToken: cancellationToken);
            }
            finally
            {
                _publishLock.Release();
            }
        }
    }
}
