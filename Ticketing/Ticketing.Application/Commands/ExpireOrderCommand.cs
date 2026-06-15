using MediatR;

namespace Eventbox.TicketingApplication.Commands
{
    public class ExpireOrderCommand : IRequest<bool>
    {
        public Guid OrderId { get; init; }
    }
}
