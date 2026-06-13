using Eventbox.Registration.Application.Abstractions.Repositories;
using Eventbox.Registration.Application.Dtos;
using Mapster;
using MediatR;

namespace Eventbox.Registration.Application.Queries;

public class GetSeatAvailabilityQueryHandler(ISeatAvailabilityRepository repository)
    : IRequestHandler<GetSeatAvailabilityQuery, SeatAvailabilityDto?>
{
    public async Task<SeatAvailabilityDto?> Handle(GetSeatAvailabilityQuery request, CancellationToken cancellationToken)
        => (await repository.GetByEventIdAsync(request.EventId, cancellationToken))?.Adapt<SeatAvailabilityDto>();
}
