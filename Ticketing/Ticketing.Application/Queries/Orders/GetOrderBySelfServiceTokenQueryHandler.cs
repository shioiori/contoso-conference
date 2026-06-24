using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Dtos.Orders;
using Mapster;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries.Orders;

public class GetOrderBySelfServiceTokenQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetOrderBySelfServiceTokenQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderBySelfServiceTokenQuery request, CancellationToken cancellationToken)
        => (await unitOfWork.Orders.GetByAccessCodeAsync(request.Token, cancellationToken))?.Adapt<OrderDto>();
}
