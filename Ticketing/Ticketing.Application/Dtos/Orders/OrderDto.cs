using Eventbox.Ticketing.Application.Dtos.Tickets;
using Eventbox.Ticketing.Domain.Enums;

namespace Eventbox.Ticketing.Application.Dtos.Orders
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid? UserId { get; set; }
        public OrderState OrderState { get; set; }
        public string? AccessCode { get; set; }
        public DateTimeOffset? ReservationExpiresAt { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public IReadOnlyCollection<OrderItemDto> Items { get; set; } = [];
        public IReadOnlyCollection<TicketDto> Tickets { get; set; } = [];
    }
}
