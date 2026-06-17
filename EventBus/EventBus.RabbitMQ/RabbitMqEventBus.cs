using Eventbox.EventBus.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Eventbox.EventBus.RabbitMQ;

public sealed class RabbitMqEventBus : IEventBus, IAsyncDisposable
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly IMessageSerializer _serializer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SubscriptionRegistry _registry;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventBus> _logger;

    private static readonly ResiliencePipeline _publishRetryPipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(2),
            BackoffType = DelayBackoffType.Exponential,
            ShouldHandle = new PredicateBuilder().Handle<Exception>()
        })
        .Build();

    private IChannel? _publishChannel; // confirm channel — BasicPublishAsync waits for broker ack
    private IChannel? _consumeChannel;

    public RabbitMqEventBus(
        RabbitMqConnectionFactory connectionFactory,
        IMessageSerializer serializer,
        IServiceScopeFactory scopeFactory,
        SubscriptionRegistry registry,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqEventBus> logger)
    {
        _connectionFactory = connectionFactory;
        _serializer = serializer;
        _scopeFactory = scopeFactory;
        _registry = registry;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        await _publishRetryPipeline.ExecuteAsync(async ct =>
        {
            _publishChannel ??= await _connectionFactory.CreateConfirmChannelAsync(ct);

            await _publishChannel.ExchangeDeclareAsync(
                exchange: _options.EventExchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: ct);

            var routingKey = GetEventRoutingKey<TEvent>();
            var body = _serializer.Serialize(@event);

            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = @event.IntegrationEventId.ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Type = @event.EventType,
                Expiration = _options.EventMessageExpirationMilliseconds.ToString(),
            };

            await _publishChannel.BasicPublishAsync(
                exchange: _options.EventExchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: ct);

            _logger.LogDebug("Published event {EventType} [{IntegrationEventId}] to exchange '{Exchange}' with routing key '{RoutingKey}'",
                @event.EventType, @event.IntegrationEventId, _options.EventExchange, routingKey);
        }, cancellationToken);
    }

    public async Task SubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var routingKey = GetEventRoutingKey<TEvent>();
        _registry.Register(routingKey, typeof(TEvent), typeof(THandler));

        _consumeChannel ??= await _connectionFactory.CreateChannelAsync(cancellationToken);

        await _consumeChannel.ExchangeDeclareAsync(
            exchange: _options.EventExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var queueName = $"{_options.QueuePrefix}.events.{routingKey}";

        await _consumeChannel.ExchangeDeclareAsync(
            exchange: _options.DeadLetterExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var deadLetterQueueName = $"{queueName}.deadletter";
        var deadLetterRoutingKey = $"{routingKey}.deadletter";
        var queueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = _options.DeadLetterExchange,
            ["x-dead-letter-routing-key"] = deadLetterRoutingKey
        };

        await _consumeChannel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments,
            cancellationToken: cancellationToken);

        await _consumeChannel.QueueDeclareAsync(
            queue: deadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _consumeChannel.QueueBindAsync(
            queue: deadLetterQueueName,
            exchange: _options.DeadLetterExchange,
            routingKey: deadLetterRoutingKey,
            cancellationToken: cancellationToken);

        await _consumeChannel.QueueBindAsync(
            queue: queueName,
            exchange: _options.EventExchange,
            routingKey: routingKey,
            cancellationToken: cancellationToken);

        await _consumeChannel.BasicQosAsync(0, _options.PrefetchCount, false, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_consumeChannel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _consumeChannel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Subscribed {Handler} to event '{RoutingKey}' on queue '{Queue}' with dead-letter queue '{DeadLetterQueue}'",
            typeof(THandler).Name, routingKey, queueName, deadLetterQueueName);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        var routingKey = args.RoutingKey;

        if (!_registry.TryGet(routingKey, out var eventType, out var handlerType))
        {
            _logger.LogWarning("No handler registered for routing key '{RoutingKey}'. Nacking.", routingKey);
            await _consumeChannel!.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
            return;
        }

        try
        {
            var @event = _serializer.Deserialize(args.Body.ToArray(), eventType);
            if (@event is null)
            {
                _logger.LogWarning("Failed to deserialize message with routing key '{RoutingKey}'.", routingKey);
                await _consumeChannel!.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService(handlerType);

            var method = handlerType.GetMethod("HandleAsync")!;
            await (Task)method.Invoke(handler, [@event, CancellationToken.None])!;

            await _consumeChannel!.BasicAckAsync(args.DeliveryTag, multiple: false);

            _logger.LogDebug("Handled event '{RoutingKey}' [{MessageId}]",
                routingKey, args.BasicProperties.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling event '{RoutingKey}' [{MessageId}]. Nacking without requeue.",
                routingKey, args.BasicProperties.MessageId);
            await _consumeChannel!.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
        }
    }

    private static string GetEventRoutingKey<TEvent>() => typeof(TEvent).Name.ToLowerInvariant();

    public async ValueTask DisposeAsync()
    {
        if (_publishChannel is not null) await _publishChannel.CloseAsync();
        if (_consumeChannel is not null) await _consumeChannel.CloseAsync();
    }
}
