using Eventbox.Ticketing.Application.Dtos.Orders;
using MediatR;

namespace Eventbox.Ticketing.Application.Commands.Orders;

public class StartPaymentCommand : IRequest<OrderAmountDto>
{
    public Guid OrderId { get; set; }
}
