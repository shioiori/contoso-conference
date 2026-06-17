using Eventbox.Ticketing.Application.Dtos;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries;

public class GetOrdersByEmailQuery : IRequest<IEnumerable<OrderDto>>
{
    public GetOrdersByEmailQuery(string email)
    {
        Email = email;
    }

    public string Email { get; }
}
