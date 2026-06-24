using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Shared.Outbox;
using Eventbox.Ticketing.Application.Abstractions;
using System.Text.Json;

namespace Eventbox.Ticketing.Infrastructure.Jobs
{
    public class OutboxProcessorJob(
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        IDelayedEventScheduler delayedEventScheduler) : IOutboxProcessorJob
    {
        private const int BatchSize = 20;

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var messages = await unitOfWork.Outbox.GetPendingAsync(BatchSize, cancellationToken);

            foreach (var message in messages)
            {
                message.Status = ProcessStatus.Processing;
                unitOfWork.Outbox.Update(message);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                try
                {
                    await ProcessAsync(message, cancellationToken);
                    message.Status = ProcessStatus.Processed;
                    message.ProcessedOnUtc = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    message.RetryCount++;
                    message.Error = ex.Message;
                    message.Status = ProcessStatus.Failed;
                }

                unitOfWork.Outbox.Update(message);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken)
        {
            switch (message.IntegrationEventType)
            {
                case nameof(OrderExpirationDueMessageIntegrationEvent):
                    var expirationEvent = JsonSerializer.Deserialize<OrderExpirationDueMessageIntegrationEvent>(message.Content)
                        ?? throw new InvalidOperationException($"Failed to deserialize {message.IntegrationEventType}");
                    await delayedEventScheduler.ScheduleAsync(expirationEvent, expirationEvent.ExpiresAt, cancellationToken);
                    break;

                case nameof(OrderConfirmedIntegrationEvent):
                    var confirmedEvent = JsonSerializer.Deserialize<OrderConfirmedIntegrationEvent>(message.Content)
                        ?? throw new InvalidOperationException($"Failed to deserialize {message.IntegrationEventType}");
                    await eventBus.PublishAsync(confirmedEvent, cancellationToken);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown integration event type: {message.IntegrationEventType}");
            }
        }
    }
}
