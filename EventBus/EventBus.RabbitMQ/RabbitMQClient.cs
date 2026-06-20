using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ
{
    public class RabbitMQClient
    {
        private readonly RabbitMQOptions _options;
        private readonly ILogger<RabbitMQClient> _logger;
        private IConnection _connection = default!;
        public IConnection Connection => _connection;
        private IChannel _publishChannel = default!;
        public IChannel PublishChannel => _publishChannel;
        private IChannel _consumeChannel = default!;
        public IChannel ConsumeChannel => _consumeChannel;

        public RabbitMQClient(RabbitMQOptions options, ILogger<RabbitMQClient> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task Init()
        {
            await ConnectAsync();
            await DeclareAsync();
            await CheckUnroutedQueuesAsync();
            await _consumeChannel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 10,
                global: false);
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
                .WaitAndRetryAsync(
                    5,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (exception, delay, attempt, _) =>
                    {
                        _logger.LogWarning(
                            exception,
                            "RabbitMQ connection attempt {Attempt} failed. Retrying in {Delay}. Host={Host}, Port={Port}, VHost={VirtualHost}",
                            attempt,
                            delay,
                            _options.Host,
                            _options.Port,
                            _options.VirtualHost);
                    });

            await retryPolicy.ExecuteAsync(async () =>
            {
                IConnection? connection = null;
                IChannel? publishChannel = null;
                IChannel? consumeChannel = null;

                try
                {
                    _logger.LogInformation(
                        "Connecting to RabbitMQ. Host={Host}, Port={Port}, VHost={VirtualHost}",
                        _options.Host,
                        _options.Port,
                        _options.VirtualHost);

                    connection = await factory.CreateConnectionAsync();
                    publishChannel = await connection.CreateChannelAsync(new CreateChannelOptions(
                        publisherConfirmationsEnabled: true,
                        publisherConfirmationTrackingEnabled: true));
                    consumeChannel = await connection.CreateChannelAsync();

                    _connection = connection;
                    _publishChannel = publishChannel;
                    _consumeChannel = consumeChannel;

                    _logger.LogInformation(
                        "RabbitMQ connection and channels established. Host={Host}, Port={Port}, VHost={VirtualHost}",
                        _options.Host,
                        _options.Port,
                        _options.VirtualHost);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "RabbitMQ connection/channel initialization failed. Host={Host}, Port={Port}, VHost={VirtualHost}",
                        _options.Host,
                        _options.Port,
                        _options.VirtualHost);

                    await CloseQuietlyAsync(consumeChannel);
                    await CloseQuietlyAsync(publishChannel);
                    await CloseQuietlyAsync(connection);
                    throw;
                }
            });
        }

        private async Task CloseQuietlyAsync(IChannel? channel)
        {
            if (channel is null)
                return;

            try
            {
                if (channel.IsOpen)
                    await channel.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Ignoring RabbitMQ channel close failure during cleanup.");
            }
        }

        private async Task CloseQuietlyAsync(IConnection? connection)
        {
            if (connection is null)
                return;

            try
            {
                if (connection.IsOpen)
                    await connection.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Ignoring RabbitMQ connection close failure during cleanup.");
            }
        }

        public async Task DeclareAsync()
        {
            var allMappings = _options.PublishMappings.Concat(_options.SubscribeMappings);

            foreach (var exchange in allMappings.DistinctBy(m => m.Exchange))
            {
                var unroutedExchange = GetUnroutedExchange(exchange.Exchange);
                var unroutedQueue = GetUnroutedQueue(exchange.Exchange);

                await _consumeChannel.ExchangeDeclareAsync(
                    exchange: unroutedExchange,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false);

                await _consumeChannel.QueueDeclareAsync(
                    queue: unroutedQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                await _consumeChannel.QueueBindAsync(
                    queue: unroutedQueue,
                    exchange: unroutedExchange,
                    routingKey: string.Empty);

                await _consumeChannel.ExchangeDeclareAsync(
                    exchange: exchange.Exchange,
                    type: exchange.ExchangeType,
                    durable: true,
                    autoDelete: false,
                    arguments: new Dictionary<string, object?>
                    {
                        ["alternate-exchange"] = unroutedExchange
                    });
            }

            var extraQueueNames = _options.ExtraQueues.Select(q => q.Queue).ToHashSet();

            foreach (var mapping in _options.SubscribeMappings)
            {
                if (!extraQueueNames.Contains(mapping.Queue))
                    await _consumeChannel.QueueDeclareAsync(
                        queue: mapping.Queue,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                await _consumeChannel.QueueBindAsync(mapping.Queue, mapping.Exchange, mapping.RoutingKey, null);

                await _consumeChannel.QueueDeclareAsync(
                    mapping.DelayQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object?>
                    {
                        ["x-dead-letter-exchange"] = mapping.Exchange,
                        ["x-dead-letter-routing-key"] = mapping.RoutingKey
                    });

                await _consumeChannel.QueueDeclareAsync(
                    queue: mapping.DeadLetterQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
            }

            foreach (var extra in _options.ExtraQueues)
            {
                await _consumeChannel.QueueDeclareAsync(
                    queue: extra.Queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: extra.Arguments.Count > 0 ? extra.Arguments : null);
                await _consumeChannel.QueueBindAsync(extra.Queue, extra.Exchange, extra.RoutingKey, null);
            }
        }

        private async Task CheckUnroutedQueuesAsync()
        {
            var exchanges = _options.PublishMappings
                .Concat(_options.SubscribeMappings)
                .DistinctBy(m => m.Exchange)
                .Select(m => m.Exchange);

            foreach (var exchange in exchanges)
            {
                var queue = GetUnroutedQueue(exchange);
                var result = await _consumeChannel.QueueDeclarePassiveAsync(queue);
                if (result.MessageCount > 0)
                {
                    _logger.LogError(
                        "RabbitMQ unrouted queue has {MessageCount} messages. Queue={Queue}",
                        result.MessageCount,
                        queue);
                }
            }
        }

        private static string GetUnroutedExchange(string exchange) => $"{exchange}.unrouted";

        private static string GetUnroutedQueue(string exchange) => $"{exchange}.unrouted.queue";
    }
}
