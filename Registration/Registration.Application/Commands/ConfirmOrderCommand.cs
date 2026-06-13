using MediatR;

namespace Eventbox.Registration.Application.Commands
{
    public class ConfirmOrderCommand : IRequest<bool>
    {
        public Guid OrderId { get; init; }
    }
}