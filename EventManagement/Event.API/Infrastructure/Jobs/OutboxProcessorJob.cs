using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Eventbox.Shared.Outbox;
using System.Text.Json;

namespace Eventbox.EventManagement.Infrastructure.Jobs
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
            message.IntergrationEventType switch
            {
                nameof(TicketTypeCreatedEvent) =>
                    JsonSerializer.Deserialize<TicketTypeCreatedEvent>(message.Content)
                    ?? throw new InvalidOperationException($"Failed to deserialize {message.IntergrationEventType}"),
                nameof(TicketCapacityAddedEvent) =>
                    JsonSerializer.Deserialize<TicketCapacityAddedEvent>(message.Content)
                    ?? throw new InvalidOperationException($"Failed to deserialize {message.IntergrationEventType}"),
                nameof(EventCreatedEvent) =>
                    JsonSerializer.Deserialize<EventCreatedEvent>(message.Content)
                    ?? throw new InvalidOperationException($"Failed to deserialize {message.IntergrationEventType}"),
                nameof(EventUpdatedEvent) =>
                    JsonSerializer.Deserialize<EventUpdatedEvent>(message.Content)
                    ?? throw new InvalidOperationException($"Failed to deserialize {message.IntergrationEventType}"),
                nameof(EventPublishedEvent) =>
                    JsonSerializer.Deserialize<EventPublishedEvent>(message.Content)
                    ?? throw new InvalidOperationException($"Failed to deserialize {message.IntergrationEventType}"),
                nameof(EventUnpublishedEvent) =>
                    JsonSerializer.Deserialize<EventUnpublishedEvent>(message.Content)
                    ?? throw new InvalidOperationException($"Failed to deserialize {message.IntergrationEventType}"),
                _ => throw new InvalidOperationException($"Unknown integration event type: {message.IntergrationEventType}")
            };
    }
}
