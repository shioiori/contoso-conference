using Amazon.SQS.Model;
using Eventbox.EventBus.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EventBus.Aws
{
    public class AwsEventBus : IEventBus, IDelayedEventScheduler, IHostedService
    {
        private readonly AwsClient _awsClient;
        private readonly AwsOptions _awsOptions;
        private readonly ILogger<AwsEventBus> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeProvider _timeProvider;

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
            await _awsClient.SnsClient.PublishAsync(mapping.TopicArn, JsonSerializer.Serialize(@event, eventType), cancellationToken);
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
                MessageBody = JsonSerializer.Serialize(@event, eventType)
            });
            _logger.LogInformation($"Scheduled {eventType.Name} for delivery in {delaySeconds} seconds");
        }

        public async Task SubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
            where TEvent : IIntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>
        {
            var eventType = typeof(TEvent);
            var mapping = _awsOptions.SubscribeMappings.Find(mapping => mapping.EventType == eventType);
            if (mapping == null)
            {
                _logger.LogWarning($"No subscribe mapping found for event type: {eventType.Name}");
                return;
            }
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var responses = await _awsClient.SqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                    {
                        QueueUrl = mapping.QueueUrl,
                        MaxNumberOfMessages = 10,
                        WaitTimeSeconds = 20
                    }, cancellationToken);

                    _logger.LogInformation($"Received {responses.Messages.Count} messages for {eventType.Name}");

                    foreach (var message in responses.Messages)
                    {
                        try
                        {
                            var @event = JsonSerializer.Deserialize<TEvent>(message.Body);
                            using var scope = _serviceProvider.CreateScope();
                            var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                            await handler.HandleAsync(@event, CancellationToken.None);
                            await _awsClient.SqsClient.DeleteMessageAsync(mapping.QueueUrl, message.ReceiptHandle, cancellationToken);
                            _logger.LogInformation($"Handled {eventType.Name} with {typeof(THandler).Name}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to handle message for {eventType.Name}, will retry");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error receiving messages for {eventType.Name}");
                    await Task.Delay(5000, cancellationToken); 
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
