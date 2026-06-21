using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;

namespace Eventbox.Ticketing.Application.IntegrationEventHandlers;

public class TicketTypeDeletedEventHandler(
    ITicketAvailabilityRepository ticketAvailabilityRepository,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<TicketTypeDeletedEvent>
{
    public async Task HandleAsync(TicketTypeDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        var availability = await ticketAvailabilityRepository.GetByEventIdAsync(@event.EventId, cancellationToken);
        if (availability is null)
            return;

        availability.RemoveTicketType(@event.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
