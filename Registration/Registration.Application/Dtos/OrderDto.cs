using Eventbox.Registration.Domain.Enums;

namespace Eventbox.Registration.Application.Dtos
{
    public class OrderDto
    {
        public Guid Id { get; init; }
        public Guid EventId { get; init; }
        public Guid? UserId { get; init; }
        public OrderState OrderState { get; init; }
        public string? AccessCode { get; init; }
        public DateTimeOffset? ReservationExpiresAt { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public IReadOnlyCollection<OrderItemDto> Items { get; init; } = Array.Empty<OrderItemDto>();
    }
}
