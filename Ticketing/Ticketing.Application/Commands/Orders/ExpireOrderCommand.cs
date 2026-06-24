using MediatR;

namespace Eventbox.Ticketing.Application.Commands.Orders
{
    public class ExpireOrderCommand : IRequest<bool>
    {
        public Guid OrderId { get; init; }
    }
}
