using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Contracts.IntegrationEvents;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;

namespace Eventbox.Ticketing.Application.IntegrationEventHandlers;

public class EventPublishedEventHandler(
    IEventScheduleRepository eventScheduleRepository,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<EventPublishedEvent>
{
    public async Task HandleAsync(EventPublishedEvent @event, CancellationToken cancellationToken = default)
    {
        await eventScheduleRepository.UpsertAsync(@event.EventId, null, null, true, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
