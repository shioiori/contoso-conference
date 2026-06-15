using Eventbox.TicketingApplication.Dtos;
using MediatR;

namespace Eventbox.TicketingApplication.Queries;

public class GetOrdersByEmailQuery : IRequest<IEnumerable<OrderDto>>
{
    public GetOrdersByEmailQuery(string email)
    {
        Email = email;
    }

    public string Email { get; }
}
