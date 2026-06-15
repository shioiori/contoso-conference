using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.Dtos;
using Mapster;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries;

public class GetTicketAvailabilityQueryHandler(ITicketAvailabilityRepository repository)
    : IRequestHandler<GetTicketAvailabilityQuery, TicketAvailabilityDto?>
{
    public async Task<TicketAvailabilityDto?> Handle(GetTicketAvailabilityQuery request, CancellationToken cancellationToken)
        => (await repository.GetByEventIdAsync(request.EventId, cancellationToken))?.Adapt<TicketAvailabilityDto>();
}
