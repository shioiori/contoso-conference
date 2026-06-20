using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Contracts.IntegrationEvents;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;

namespace Eventbox.Ticketing.Application.IntegrationEventHandlers;

public class EventUnpublishedEventHandler(
    IEventScheduleRepository eventScheduleRepository,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<EventUnpublishedEvent>
{
    public async Task HandleAsync(EventUnpublishedEvent @event, CancellationToken cancellationToken = default)
    {
        await eventScheduleRepository.UpsertAsync(@event.EventId, null, null, false, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
