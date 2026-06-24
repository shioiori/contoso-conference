using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Contracts.IntegrationEvents;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Shared.Exceptions;

namespace Eventbox.Ticketing.Application.IntegrationEventHandlers.Inventory;

public class TicketCapacityAddedEventHandler(
    ITicketTypeAvailabilityRepository ticketAvailabilityRepository,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<TicketCapacityAddedIntegrationEvent>
{
    public async Task HandleAsync(TicketCapacityAddedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var availability = await ticketAvailabilityRepository.GetByIdAsync(@event.Id, cancellationToken)
            ?? throw new NotFoundException($"Ticket availability for event '{@event.EventId}' was not found.");

        availability.UpdateQuantity(@event.NewQuantity);
        ticketAvailabilityRepository.Update(availability);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
