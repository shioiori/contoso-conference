using Eventbox.Ticketing.Application.Dtos;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries;

public class GetOrderBySelfServiceTokenQuery : IRequest<OrderDto?>
{
    public GetOrderBySelfServiceTokenQuery(string token)
    {
        Token = token;
    }

    public string Token { get; }
}
