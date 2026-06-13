using Eventbox.EventBus.Core.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Eventbox.EventBus.RabbitMQ;

public sealed class RabbitMqDelayedEventScheduler : IDelayedEventScheduler, IAsyncDisposable
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly IMessageSerializer _serializer;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqDelayedEventScheduler> _logger;

    private IChannel? _channel;

    public RabbitMqDelayedEventScheduler(
        RabbitMqConnectionFactory connectionFactory,
        IMessageSerializer serializer,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqDelayedEventScheduler> logger)
    {
        _connectionFactory = connectionFactory;
        _serializer = serializer;
        _options = options.Value;
        _logger = logger;
    }

    public async Task ScheduleAsync<TEvent>(
        TEvent @event,
        DateTimeOffset deliverAt,
        CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        _channel ??= await _connectionFactory.CreateChannelAsync(cancellationToken);

        var routingKey = GetEventRoutingKey<TEvent>();
        var delayQueueName = $"{_options.QueuePrefix}.delay.{routingKey}";

        await _channel.ExchangeDeclareAsync(
            exchange: _options.EventExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var queueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = _options.EventExchange,
            ["x-dead-letter-routing-key"] = routingKey
        };

        await _channel.QueueDeclareAsync(
            queue: delayQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments,
            cancellationToken: cancellationToken);

        var delay = deliverAt - DateTimeOffset.UtcNow;
        var ttlMilliseconds = Math.Max(0, (long)delay.TotalMilliseconds);
        var body = _serializer.Serialize(@event);

        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = @event.IntegrationEventId.ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            Type = @event.EventType,
            Expiration = ttlMilliseconds.ToString()
        };

        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: delayQueueName,
            mandatory: true,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Scheduled delayed event {EventType} [{IntegrationEventId}] for {DeliverAt} with TTL {TtlMilliseconds}ms",
            @event.EventType,
            @event.IntegrationEventId,
            deliverAt,
            ttlMilliseconds);
    }

    private static string GetEventRoutingKey<TEvent>() => typeof(TEvent).Name.ToLowerInvariant();

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.CloseAsync();
    }
}
