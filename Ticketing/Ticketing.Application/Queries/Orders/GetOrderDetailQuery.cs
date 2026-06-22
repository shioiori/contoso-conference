using Eventbox.Ticketing.Application.Dtos.Orders;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries.Orders;

public class GetOrderDetailQuery : IRequest<OrderDto>
{
    public GetOrderDetailQuery(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}
