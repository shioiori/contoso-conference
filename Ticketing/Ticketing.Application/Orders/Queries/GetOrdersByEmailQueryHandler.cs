using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Dtos;
using Mapster;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries;

public class GetOrdersByEmailQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetOrdersByEmailQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByEmailQuery request, CancellationToken cancellationToken)
        => (await unitOfWork.Orders.GetByEmailAsync(request.Email, cancellationToken)).Adapt<IEnumerable<OrderDto>>();
}
