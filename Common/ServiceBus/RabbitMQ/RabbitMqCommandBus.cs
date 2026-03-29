using Contoso.ServiceBus.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Contoso.ServiceBus.RabbitMQ;

public sealed class RabbitMqCommandBus : ICommandBus, IAsyncDisposable
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly IMessageSerializer _serializer;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqCommandBus> _logger;

    private IChannel? _channel;

    public RabbitMqCommandBus(
        RabbitMqConnectionFactory connectionFactory,
        IMessageSerializer serializer,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqCommandBus> logger)
    {
        _connectionFactory = connectionFactory;
        _serializer = serializer;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        _channel ??= await _connectionFactory.CreateChannelAsync(cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _options.CommandExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var routingKey = GetCommandRoutingKey<TCommand>();

        var queueName = $"{_options.QueuePrefix}.commands.{routingKey}";
        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: _options.CommandExchange,
            routingKey: routingKey,
            cancellationToken: cancellationToken);

        var body = _serializer.Serialize(command);

        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = command.CommandId.ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            Type = typeof(TCommand).Name,
        };

        await _channel.BasicPublishAsync(
            exchange: _options.CommandExchange,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogDebug("Sent command {CommandType} [{CommandId}] to exchange '{Exchange}' routing key '{RoutingKey}'",
            typeof(TCommand).Name, command.CommandId, _options.CommandExchange, routingKey);
    }

    private static string GetCommandRoutingKey<TCommand>() => typeof(TCommand).Name.ToLowerInvariant();

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.CloseAsync();
    }
}
