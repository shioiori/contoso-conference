using Eventbox.Registration.Application.Dtos;
using MediatR;

namespace Eventbox.Registration.Application.Queries;

public class GetSeatAvailabilityQuery : IRequest<SeatAvailabilityDto?>
{
    public GetSeatAvailabilityQuery(Guid eventId)
    {
        EventId = eventId;
    }

    public Guid EventId { get; }
}
