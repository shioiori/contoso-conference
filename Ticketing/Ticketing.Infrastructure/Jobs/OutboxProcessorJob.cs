using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Shared.Outbox;
using Eventbox.Ticketing.Application.Abstractions;
using System.Text.Json;

namespace Eventbox.Ticketing.Infrastructure.Jobs
{
    public class OutboxProcessorJob(
        IUnitOfWork unitOfWork,
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
                    var @event = Deserialize(message) as OrderExpirationDueMessageIntergrationEvent;
                    await delayedEventScheduler.ScheduleAsync(@event, @event.ExpiresAt, cancellationToken);
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

        private static IIntegrationEvent Deserialize(OutboxMessage message) =>
            message.IntegrationEventType switch
            {
                nameof(OrderExpirationDueMessageIntergrationEvent) =>
                    JsonSerializer.Deserialize<OrderExpirationDueMessageIntergrationEvent>(message.Content)
                    ?? throw new InvalidOperationException($"Failed to deserialize {message.IntegrationEventType}"),
                _ => throw new InvalidOperationException($"Unknown integration event type: {message.IntegrationEventType}")
            };
    }
}
