using Eventbox.TicketingApplication.Abstractions.Repositories;
using Eventbox.TicketingApplication.Dtos;
using Mapster;
using MediatR;

namespace Eventbox.TicketingApplication.Queries;

public class GetOrdersByEmailQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrdersByEmailQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByEmailQuery request, CancellationToken cancellationToken)
        => (await orderRepository.GetByEmailAsync(request.Email, cancellationToken)).Adapt<IEnumerable<OrderDto>>();
}
