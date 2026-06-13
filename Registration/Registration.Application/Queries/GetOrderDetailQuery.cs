using Eventbox.Registration.Application.Dtos;
using MediatR;

namespace Eventbox.Registration.Application.Queries;

public class GetOrderDetailQuery : IRequest<OrderDto?>
{
    public GetOrderDetailQuery(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}
