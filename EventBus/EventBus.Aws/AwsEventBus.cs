using Amazon.SQS.Model;
using Eventbox.EventBus.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;
using SnsModel = Amazon.SimpleNotificationService.Model;

namespace EventBus.Aws
{
    public class AwsEventBus : IEventBus, IDelayedEventScheduler, IHostedService
    {
        private const string EventTypeAttribute = "EventType";

        private readonly AwsClient _awsClient;
        private readonly AwsOptions _awsOptions;
        private readonly ILogger<AwsEventBus> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeProvider _timeProvider;

        // queueUrl -> (eventTypeName -> dispatch delegate that deserializes + handles the payload)
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Func<string, CancellationToken, Task>>> _dispatchers = new();
        // queueUrl -> the single consumer loop polling that queue
        private readonly ConcurrentDictionary<string, Task> _consumers = new();
        private readonly object _consumerLock = new();

        public AwsEventBus(IOptions<AwsOptions> options, ILogger<AwsEventBus> logger, IServiceProvider serviceProvider, TimeProvider timeProvider)
        {
            _awsOptions = options.Value;
            _awsClient = new AwsClient(_awsOptions);
            _logger = logger;
            _serviceProvider = serviceProvider;
            _timeProvider = timeProvider;
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent
        {
            var eventType = @event.GetType();
            var mapping = _awsOptions.PublishMappings.Find(mapping => mapping.EventType == eventType);
            if (mapping == null)
            {
                _logger.LogWarning($"No publish mapping found for event type: {eventType.Name}");
                return;
            }
            await _awsClient.SnsClient.PublishAsync(new SnsModel.PublishRequest
            {
                TopicArn = mapping.TopicArn,
                Message = JsonSerializer.Serialize(@event, eventType),
                MessageAttributes = new Dictionary<string, SnsModel.MessageAttributeValue>
                {
                    [EventTypeAttribute] = new() { DataType = "String", StringValue = eventType.Name }
                }
            }, cancellationToken);
            _logger.LogInformation($"Published {eventType.Name} to {mapping.TopicArn}");
        }

        public async Task ScheduleAsync<TEvent>(TEvent @event, DateTimeOffset deliverAt, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent
        {
            var eventType = @event.GetType();
            var mapping = _awsOptions.SubscribeMappings.Find(mapping => mapping.EventType == eventType);
            if (mapping == null)
            {
                _logger.LogWarning($"No subscribe mapping found for event type: {eventType.Name}");
                return;
            }
            var delaySeconds = Math.Clamp((int)(deliverAt - _timeProvider.GetUtcNow()).TotalSeconds, 0, 900);
            await _awsClient.SqsClient.SendMessageAsync(new SendMessageRequest
            {
                DelaySeconds = delaySeconds,
                QueueUrl = mapping.QueueUrl,
                MessageBody = JsonSerializer.Serialize(@event, eventType),
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    [EventTypeAttribute] = new() { DataType = "String", StringValue = eventType.Name }
                }
            }, cancellationToken);
            _logger.LogInformation($"Scheduled {eventType.Name} for delivery in {delaySeconds} seconds");
        }

        public Task SubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
            where TEvent : IIntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>
        {
            var eventType = typeof(TEvent);
            var mapping = _awsOptions.SubscribeMappings.Find(mapping => mapping.EventType == eventType);
            if (mapping == null)
            {
                _logger.LogWarning($"No subscribe mapping found for event type: {eventType.Name}");
                return Task.CompletedTask;
            }

            var queueDispatchers = _dispatchers.GetOrAdd(mapping.QueueUrl, _ => new());
            queueDispatchers[eventType.Name] = async (payload, token) =>
            {
                var @event = JsonSerializer.Deserialize<TEvent>(payload)
                    ?? throw new InvalidOperationException($"Failed to deserialize {eventType.Name}");
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                await handler.HandleAsync(@event, token);
            };

            // start a single consumer loop per queue, regardless of how many event types share it.
            if (!_consumers.ContainsKey(mapping.QueueUrl))
            {
                lock (_consumerLock)
                {
                    if (!_consumers.ContainsKey(mapping.QueueUrl))
                        _consumers[mapping.QueueUrl] = ConsumeQueueAsync(mapping.QueueUrl, cancellationToken);
                }
            }

            return Task.CompletedTask;
        }

        private async Task ConsumeQueueAsync(string queueUrl, CancellationToken cancellationToken)
        {
            var dispatchers = _dispatchers[queueUrl];
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var response = await _awsClient.SqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                        {
                            QueueUrl = queueUrl,
                            MaxNumberOfMessages = 10,
                            WaitTimeSeconds = 20,
                            MessageAttributeNames = new List<string> { "All" }
                        }, cancellationToken);

                        // AWS SDK v4 returns null (not an empty list) when no messages are available.
                        var messages = response.Messages ?? new List<Message>();

                        foreach (var message in messages)
                        {
                            try
                            {
                                var (eventTypeName, payload) = ParseMessage(message);
                                if (eventTypeName == null || !dispatchers.TryGetValue(eventTypeName, out var dispatch))
                                {
                                    _logger.LogWarning($"No handler registered for message type '{eventTypeName}' on {queueUrl}");
                                    continue;
                                }
                                await dispatch(payload, CancellationToken.None);
                                await _awsClient.SqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, cancellationToken);
                                _logger.LogInformation($"Handled {eventTypeName} from {queueUrl}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Failed to handle message from {queueUrl}, will retry");
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error receiving messages from {queueUrl}");
                        await Task.Delay(5000, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static (string? eventTypeName, string payload) ParseMessage(Message message)
        {
            var body = message.Body;

            // SNS->SQS without Raw Message Delivery: body is an SNS envelope carrying the payload
            // and the published message attributes.
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object
                    && root.TryGetProperty("Type", out var type)
                    && type.GetString() == "Notification"
                    && root.TryGetProperty("Message", out var msg)
                    && msg.ValueKind == JsonValueKind.String)
                {
                    string? typeName = null;
                    if (root.TryGetProperty("MessageAttributes", out var attrs)
                        && attrs.ValueKind == JsonValueKind.Object
                        && attrs.TryGetProperty(EventTypeAttribute, out var attr)
                        && attr.TryGetProperty("Value", out var value))
                    {
                        typeName = value.GetString();
                    }
                    return (typeName, msg.GetString() ?? body);
                }
            }
            catch (JsonException)
            {
            }

            // Raw Message Delivery or a message sent directly to SQS (e.g. ScheduleAsync):
            // body is the raw payload and the type is an SQS message attribute.
            string? rawTypeName = null;
            if (message.MessageAttributes != null
                && message.MessageAttributes.TryGetValue(EventTypeAttribute, out var rawAttr))
            {
                rawTypeName = rawAttr.StringValue;
            }
            return (rawTypeName, body);
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
