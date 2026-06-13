using MediatR;

namespace Eventbox.Registration.Application.Commands
{
    public class ExpireOrderCommand : IRequest<bool>
    {
        public Guid OrderId { get; init; }
    }
}
