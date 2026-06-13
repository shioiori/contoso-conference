using Eventbox.Registration.Application.Abstractions.Repositories;
using Eventbox.Registration.Application.Dtos;
using Mapster;
using MediatR;

namespace Eventbox.Registration.Application.Queries;

public class GetOrdersByEmailQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrdersByEmailQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByEmailQuery request, CancellationToken cancellationToken)
        => (await orderRepository.GetByEmailAsync(request.Email, cancellationToken)).Adapt<IEnumerable<OrderDto>>();
}
