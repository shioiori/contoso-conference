using Eventbox.Ticketing.Application.Dtos.Inventory;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries.Inventory;

public class GetTicketAvailabilityQuery : IRequest<IReadOnlyList<TicketTypeAvailabilityDto>>
{
    public GetTicketAvailabilityQuery(Guid eventId)
    {
        EventId = eventId;
    }

    public Guid EventId { get; }
}
