using Eventbox.Registration.Application.Dtos;
using MediatR;

namespace Eventbox.Registration.Application.Queries;

public class GetOrdersByEmailQuery : IRequest<IEnumerable<OrderDto>>
{
    public GetOrdersByEmailQuery(string email)
    {
        Email = email;
    }

    public string Email { get; }
}
