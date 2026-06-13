using Eventbox.Registration.Application.Dtos;
using MediatR;

namespace Eventbox.Registration.Application.Queries;

public class GetOrderBySelfServiceTokenQuery : IRequest<OrderDto?>
{
    public GetOrderBySelfServiceTokenQuery(string token)
    {
        Token = token;
    }

    public string Token { get; }
}
