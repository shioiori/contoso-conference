using Eventbox.Registration.Application.Abstractions;
using Eventbox.Registration.Application.Dtos;
using Mapster;
using MediatR;

namespace Eventbox.Registration.Application.Queries;

public class GetOrderDetailQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetOrderDetailQuery, OrderDto?>
{
    public Task<OrderDto?> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
        => Task.FromResult(unitOfWork.Orders
            .Get(x => x.Id == request.OrderId, includeProperties: "OrderItems")
            .FirstOrDefault()
            ?.Adapt<OrderDto>());
}
