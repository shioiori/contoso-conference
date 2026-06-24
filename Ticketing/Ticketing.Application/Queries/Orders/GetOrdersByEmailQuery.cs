using Eventbox.Ticketing.Application.Dtos.Orders;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries.Orders;

public class GetOrdersByEmailQuery : IRequest<IEnumerable<OrderDto>>
{
    public GetOrdersByEmailQuery(string email)
    {
        Email = email;
    }

    public string Email { get; }
}
