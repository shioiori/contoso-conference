using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Contracts.IntegrationEvents;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Domain.Entities.TicketAvailabilityAggregate;
using Eventbox.Ticketing.Domain.Enums;

namespace Eventbox.Ticketing.Application.IntegrationEventHandlers;

public class TicketTypeCreatedEventHandler(
    ITicketAvailabilityRepository ticketAvailabilityRepository,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<TicketTypeCreatedEvent>
{
    public async Task HandleAsync(TicketTypeCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var visibility = Enum.TryParse<TicketVisibility>(@event.Visibility, ignoreCase: true, out var parsedVisibility)
            ? parsedVisibility
            : TicketVisibility.Public;

        var ticketType = new TicketTypeAvailability(
            @event.Id,
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

        var availability = await ticketAvailabilityRepository.GetByEventIdAsync(@event.EventId, cancellationToken);
        if (availability is null)
        {
            availability = new TicketAvailability(@event.EventId, [ticketType]);
            await ticketAvailabilityRepository.AddAsync(availability, cancellationToken);
        }
        else if (!availability.TicketTypes.Any(existing => existing.Id == @event.Id))
        {
            availability.AddTicketType(ticketType);
            ticketAvailabilityRepository.Update(availability);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
