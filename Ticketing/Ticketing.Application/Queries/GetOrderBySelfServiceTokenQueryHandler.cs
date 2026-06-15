using Eventbox.TicketingApplication.Abstractions.Repositories;
using Eventbox.TicketingApplication.Dtos;
using Mapster;
using MediatR;

namespace Eventbox.TicketingApplication.Queries;

public class GetOrderBySelfServiceTokenQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderBySelfServiceTokenQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderBySelfServiceTokenQuery request, CancellationToken cancellationToken)
        => (await orderRepository.GetByAccessCodeAsync(request.Token, cancellationToken))?.Adapt<OrderDto>();
}
