using Eventbox.Ticketing.Application.Dtos;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries;

public class GetOrderDetailQuery : IRequest<OrderDto?>
{
    public GetOrderDetailQuery(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}
