using Eventbox.TicketingApplication.Dtos;
using MediatR;

namespace Eventbox.TicketingApplication.Queries;

public class GetTicketAvailabilityQuery : IRequest<TicketAvailabilityDto?>
{
    public GetTicketAvailabilityQuery(Guid eventId)
    {
        EventId = eventId;
    }

    public Guid EventId { get; }
}
