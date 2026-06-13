using Eventbox.Registration.Application.Abstractions.Repositories;
using Eventbox.Registration.Application.Dtos;
using Mapster;
using MediatR;

namespace Eventbox.Registration.Application.Queries;

public class GetOrderBySelfServiceTokenQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderBySelfServiceTokenQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderBySelfServiceTokenQuery request, CancellationToken cancellationToken)
        => (await orderRepository.GetByAccessCodeAsync(request.Token, cancellationToken))?.Adapt<OrderDto>();
}
