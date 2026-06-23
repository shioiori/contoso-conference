using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Contracts.IntegrationEvents;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Domain.Inventory;
using Eventbox.Ticketing.Domain.Enums;
using Eventbox.Shared.Exceptions;

namespace Eventbox.Ticketing.Application.IntegrationEventHandlers.Inventory;

public class TicketTypeCreatedEventHandler(
    ITicketTypeAvailabilityRepository ticketTypeAvailabilityRepository,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<TicketTypeCreatedEvent>
{
    public async Task HandleAsync(TicketTypeCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var visibility = Enum.TryParse<TicketVisibility>(@event.Visibility, ignoreCase: true, out var parsedVisibility)
            ? parsedVisibility
            : TicketVisibility.Public;

        var ticketType = new TicketTypeAvailability(
            @event.Id,
            @event.EventId,
            @event.Name,
            @event.Quantity,
            @event.Currency,
            @event.MinPerOrder,
            @event.MaxPerOrder,
            visibility,
            @event.AccessCodeHash,
            @event.PricingPhases.Select(phase => new PricingPhaseAvailability(
                phase.Id,
                phase.Name,
                phase.Price,
                phase.StartTime,
                phase.EndTime)));

        var availability = await ticketTypeAvailabilityRepository.GetByIdAsync(@event.Id, cancellationToken);
        if (availability == null)
        {
            await ticketTypeAvailabilityRepository.AddAsync(ticketType, cancellationToken);
        }
        else
        {
            throw new ConflictException($"Ticket type availability with id {@event.Id} is already exists.");
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
