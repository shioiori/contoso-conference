using MediatR;

namespace Eventbox.Ticketing.Application.Commands.Orders
{
    public class CancelOrderCommand : IRequest<bool>
    {
        public Guid OrderId { get; init; }
    }
}
