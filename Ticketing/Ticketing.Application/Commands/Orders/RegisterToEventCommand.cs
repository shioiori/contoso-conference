using MediatR;
using Eventbox.Ticketing.Application.Dtos.Orders;

namespace Eventbox.Ticketing.Application.Commands.Orders
{
    public class RegisterToEventCommand : IRequest<OrderDto>
    {
        public Guid EventId { get; init; }
        public Guid? UserId { get; init; }
        public string Name { get; init; }
        public string Email { get; init; } 
        public Guid TicketTypeId { get; init; }
        public int Quantity { get; init; } = 1;
        public string? AccessCode { get; init; }
    }
}
