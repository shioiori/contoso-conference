namespace Eventbox.Registration.Application.Dtos
{
    public class OrderItemDto
    {
        public Guid Id { get; init; }
        public int TicketTypeId { get; init; }
        public int Quantity { get; init; }
    }
}
