using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Contracts.IntegrationEvents;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;

namespace Eventbox.Ticketing.Application.IntegrationEventHandlers;

public class EventPublishedEventHandler(
    IEventSnapshotRepository eventSnapshotRepository,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<EventPublishedEvent>
{
    public async Task HandleAsync(EventPublishedEvent @event, CancellationToken cancellationToken = default)
    {
        await eventSnapshotRepository.UpsertAsync(@event.EventId, null, null, true, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
