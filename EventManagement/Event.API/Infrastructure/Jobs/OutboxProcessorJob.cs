using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Eventbox.Shared.Outbox;
using System.Text.Json;

namespace Eventbox.EventManagement.EventApi.Infrastructure.Jobs
{
    public class OutboxProcessorJob(
        IUnitOfWork unitOfWork,
        IEventBus eventBus) : IOutboxProcessorJob
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
                    var @event = Deserialize(message);
                    await eventBus.PublishAsync(@event, cancellationToken);

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
                nameof(TicketTypeCreatedIntegrationEvent) or "TicketTypeCreatedEvent" =>
                    JsonSerializer.Deserialize<TicketTypeCreatedIntegrationEvent>(message.Content)
                    ?? throw new InvalidOperationException($"Failed to deserialize {message.IntegrationEventType}"),
                nameof(TicketCapacityAddedIntegrationEvent) or "TicketCapacityAddedEvent" =>
                    JsonSerializer.Deserialize<TicketCapacityAddedIntegrationEvent>(message.Content)
                    ?? throw new InvalidOperationException($"Failed to deserialize {message.IntegrationEventType}"),
                nameof(EventCreatedIntegrationEvent) or "EventCreatedEvent" =>
                    JsonSerializer.Deserialize<EventCreatedIntegrationEvent>(message.Content)
                    ?? throw new InvalidOperationException($"Failed to deserialize {message.IntegrationEventType}"),
                nameof(EventUpdatedIntegrationEvent) or "EventUpdatedEvent" =>
                    JsonSerializer.Deserialize<EventUpdatedIntegrationEvent>(message.Content)
                    ?? throw new InvalidOperationException($"Failed to deserialize {message.IntegrationEventType}"),
                nameof(EventPublishedIntegrationEvent) or "EventPublishedEvent" =>
                    JsonSerializer.Deserialize<EventPublishedIntegrationEvent>(message.Content)
                    ?? throw new InvalidOperationException($"Failed to deserialize {message.IntegrationEventType}"),
                nameof(EventUnpublishedIntegrationEvent) or "EventUnpublishedEvent" =>
                    JsonSerializer.Deserialize<EventUnpublishedIntegrationEvent>(message.Content)
                    ?? throw new InvalidOperationException($"Failed to deserialize {message.IntegrationEventType}"),
                nameof(TicketTypeDeletedIntegrationEvent) or "TicketTypeDeletedEvent" =>
                    JsonSerializer.Deserialize<TicketTypeDeletedIntegrationEvent>(message.Content)
                    ?? throw new InvalidOperationException($"Failed to deserialize {message.IntegrationEventType}"),
                _ => throw new InvalidOperationException($"Unknown integration event type: {message.IntegrationEventType}")
            };
    }
}
