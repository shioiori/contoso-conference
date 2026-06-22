using Eventbox.Ticketing.Application.Dtos.Inventory;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries.Inventory;

public class GetTicketAvailabilityQuery : IRequest<TicketAvailabilityDto?>
{
    public GetTicketAvailabilityQuery(Guid eventId)
    {
        EventId = eventId;
    }

    public Guid EventId { get; }
}
