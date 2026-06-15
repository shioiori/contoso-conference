using MediatR;
using Eventbox.TicketingApplication.Dtos;

namespace Eventbox.TicketingApplication.Commands
{
    public class RegisterToEventCommand : IRequest<OrderDto>
    {
        public Guid EventId { get; init; }
        public Guid? UserId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public int TicketTypeId { get; init; }
        public int Quantity { get; init; } = 1;
        public string? AccessCode { get; init; }
    }
}
