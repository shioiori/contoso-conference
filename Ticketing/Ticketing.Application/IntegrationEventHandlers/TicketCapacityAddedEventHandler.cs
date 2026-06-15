using Eventbox.EventBus.Core.Abstractions;
using Eventbox.TicketingApplication.Abstractions;
using Eventbox.TicketingApplication.Abstractions.Repositories;
using Eventbox.TicketingApplication.IntegrationEvents;
using Eventbox.Shared.Exceptions;

namespace Eventbox.TicketingApplication.IntegrationEventHandlers;

public class TicketCapacityAddedEventHandler(
    ITicketAvailabilityRepository ticketAvailabilityRepository,
    IRegistrationUnitOfWork unitOfWork) : IIntegrationEventHandler<TicketCapacityAddedEvent>
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
