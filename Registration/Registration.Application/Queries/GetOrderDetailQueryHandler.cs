using Eventbox.Registration.Application.Abstractions.Repositories;
using Eventbox.Registration.Application.Dtos;
using Mapster;
using MediatR;

namespace Eventbox.Registration.Application.Queries;

public class GetOrderDetailQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderDetailQuery, OrderDto?>
{
    public Task<OrderDto?> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
        => Task.FromResult(orderRepository
            .Get(x => x.Id == request.OrderId, includeProperties: "OrderItems")
            .FirstOrDefault()
            ?.Adapt<OrderDto>());
}
