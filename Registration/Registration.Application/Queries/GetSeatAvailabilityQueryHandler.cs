using Eventbox.Registration.Application.Abstractions;
using Eventbox.Registration.Application.Dtos;
using Mapster;
using MediatR;

namespace Eventbox.Registration.Application.Queries;

public class GetSeatAvailabilityQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSeatAvailabilityQuery, SeatAvailabilityDto?>
{
    public async Task<SeatAvailabilityDto?> Handle(GetSeatAvailabilityQuery request, CancellationToken cancellationToken)
        => (await unitOfWork.SeatAvailabilities.GetByEventIdAsync(request.EventId, cancellationToken))?.Adapt<SeatAvailabilityDto>();
}
