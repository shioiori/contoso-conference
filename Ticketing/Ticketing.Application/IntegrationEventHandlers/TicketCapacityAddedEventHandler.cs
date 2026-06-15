using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.IntegrationEvents;
using Eventbox.Shared.Exceptions;

namespace Eventbox.Ticketing.Application.IntegrationEventHandlers;

public class TicketCapacityAddedEventHandler(
    ITicketAvailabilityRepository ticketAvailabilityRepository,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<TicketCapacityAddedEvent>
{
    public async Task HandleAsync(TicketCapacityAddedEvent @event, CancellationToken cancellationToken = default)
    {
        var availability = await ticketAvailabilityRepository.GetByEventIdAsync(@event.EventId, cancellationToken)
            ?? throw new NotFoundException($"Ticket availability for event '{@event.EventId}' was not found.");

        availability.IncreaseTicketTypeQuantity(@event.Id, @event.AddedQuantity);
        ticketAvailabilityRepository.Update(availability);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
