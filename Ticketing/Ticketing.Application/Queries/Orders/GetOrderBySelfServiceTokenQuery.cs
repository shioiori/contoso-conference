using Eventbox.Ticketing.Application.Dtos.Orders;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries.Orders;

public class GetOrderBySelfServiceTokenQuery : IRequest<OrderDto?>
{
    public GetOrderBySelfServiceTokenQuery(string token)
    {
        Token = token;
    }

    public string Token { get; }
}
