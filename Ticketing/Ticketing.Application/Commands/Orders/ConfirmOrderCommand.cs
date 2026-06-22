using MediatR;

namespace Eventbox.Ticketing.Application.Commands.Orders
{
    public class ConfirmOrderCommand : IRequest<bool>
    {
        public Guid OrderId { get; init; }
    }
}