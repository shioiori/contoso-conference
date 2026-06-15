using MediatR;

namespace Eventbox.Ticketing.Application.Commands
{
    public class ExpireOrderCommand : IRequest<bool>
    {
        public Guid OrderId { get; init; }
    }
}
