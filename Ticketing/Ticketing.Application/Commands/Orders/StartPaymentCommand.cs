using MediatR;

namespace Eventbox.Ticketing.Application.Commands.Orders;

public class StartPaymentCommand : IRequest
{
    public Guid OrderId { get; set; }
}
