using Eventbox.TicketingApplication.Dtos;
using MediatR;

namespace Eventbox.TicketingApplication.Queries;

public class GetOrderBySelfServiceTokenQuery : IRequest<OrderDto?>
{
    public GetOrderBySelfServiceTokenQuery(string token)
    {
        Token = token;
    }

    public string Token { get; }
}
