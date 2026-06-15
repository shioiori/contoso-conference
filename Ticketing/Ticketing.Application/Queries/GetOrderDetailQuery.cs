using Eventbox.TicketingApplication.Dtos;
using MediatR;

namespace Eventbox.TicketingApplication.Queries;

public class GetOrderDetailQuery : IRequest<OrderDto?>
{
    public GetOrderDetailQuery(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}
