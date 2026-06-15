using Eventbox.Ticketing.Application.Dtos;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries;

public class GetTicketAvailabilityQuery : IRequest<TicketAvailabilityDto?>
{
    public GetTicketAvailabilityQuery(Guid eventId)
    {
        EventId = eventId;
    }

    public Guid EventId { get; }
}
