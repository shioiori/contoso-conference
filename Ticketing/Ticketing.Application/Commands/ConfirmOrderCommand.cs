using MediatR;

namespace Eventbox.TicketingApplication.Commands
{
    public class ConfirmOrderCommand : IRequest<bool>
    {
        public Guid OrderId { get; init; }
    }
}