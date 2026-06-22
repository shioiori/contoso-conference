using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.Dtos.Inventory;
using Mapster;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries.Inventory;

public class GetTicketAvailabilityQueryHandler(ITicketAvailabilityRepository repository)
    : IRequestHandler<GetTicketAvailabilityQuery, TicketAvailabilityDto?>
{
    public async Task<TicketAvailabilityDto?> Handle(GetTicketAvailabilityQuery request, CancellationToken cancellationToken)
        => (await repository.GetByEventIdAsync(request.EventId, cancellationToken))?.Adapt<TicketAvailabilityDto>();
}
