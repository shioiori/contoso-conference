using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Contracts.IntegrationEvents;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;

namespace Eventbox.Ticketing.Application.IntegrationEventHandlers.Events;

public class EventCreatedEventHandler(
    IEventSnapshotRepository eventSnapshotRepository,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<EventCreatedIntegrationEvent>
{
    public async Task HandleAsync(EventCreatedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        await eventSnapshotRepository.UpsertAsync(@event.EventId, @event.From, @event.To, @event.IsPublished, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
