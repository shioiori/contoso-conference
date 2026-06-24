using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.Dtos.Inventory;
using Mapster;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries.Inventory;

public class GetTicketAvailabilityQueryHandler(ITicketTypeAvailabilityRepository repository)
    : IRequestHandler<GetTicketAvailabilityQuery, IReadOnlyList<TicketTypeAvailabilityDto>>
{
    public async Task<IReadOnlyList<TicketTypeAvailabilityDto>> Handle(GetTicketAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var ticketTypes = await repository.GetByEventIdAsync(request.EventId, cancellationToken);
        return ticketTypes.Adapt<IReadOnlyList<TicketTypeAvailabilityDto>>();
    }
}
