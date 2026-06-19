using Eventbox.EventBus.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EventBus.RabbitMQ
{
    public class RabbitMQ
    {
        private readonly RabbitMQOptions _options;
        private IConnection _connection;
        public IConnection Connection => _connection;
        private IChannel _publishChannel;
        public IChannel PublishChannel => _publishChannel;
        private IChannel _consumeChannel;
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
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                HostName = _options.HostName
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

    public class RabbitMQEventBus : IEventBus, IHostedService
    {
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
            var mapping = _options.PublishMappings.FirstOrDefault(m => m.EventType == typeof(TEvent))
                ?? throw new InvalidOperationException($"No publish mapping registered for {typeof(TEvent).Name}");
            await _rabbitMQ.PublishChannel.BasicPublishAsync(
                mapping.Exchange, mapping.RoutingKey, true,
                body: JsonSerializer.SerializeToUtf8Bytes(@event));
        }

        public async Task SubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
            where TEvent : IIntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>
        {
            var mapping = _options.SubscribeMappings.FirstOrDefault(m => m.EventType == typeof(TEvent))
                ?? throw new InvalidOperationException($"No subscribe mapping registered for {typeof(TEvent).Name}");

            var consumer = new AsyncEventingBasicConsumer(_rabbitMQ.ConsumeChannel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var @event = JsonSerializer.Deserialize<TEvent>(body);
                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                    await handler.HandleAsync(@event, CancellationToken.None);
                    await _rabbitMQ.ConsumeChannel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch
                {
                    var retryCount = ea.BasicProperties.Headers
                        ?.TryGetValue("x-death", out var xDeath) == true
                        && xDeath is List<object> deaths
                        ? deaths.Count : 0;
                    await _rabbitMQ.ConsumeChannel.BasicNackAsync(
                        ea.DeliveryTag, multiple: false, requeue: retryCount < 3);
                }
            };

            await _rabbitMQ.ConsumeChannel.BasicConsumeAsync(mapping.Queue, autoAck: false, consumer: consumer);
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
    }
}
