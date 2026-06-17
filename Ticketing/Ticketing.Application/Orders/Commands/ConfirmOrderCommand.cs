using MediatR;

namespace Eventbox.Ticketing.Application.Commands
{
    public class ConfirmOrderCommand : IRequest<bool>
    {
        public Guid OrderId { get; init; }
    }
}