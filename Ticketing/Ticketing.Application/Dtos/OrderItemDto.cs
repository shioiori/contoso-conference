namespace Eventbox.Ticketing.Application.Dtos
{
    public class OrderItemDto
    {
        public Guid Id { get; init; }
        public int TicketTypeId { get; init; }
        public int PricingPhaseId { get; init; }
        public string TicketTypeName { get; init; } = "";
        public string PricingPhaseName { get; init; } = "";
        public decimal UnitPrice { get; init; }
        public string Currency { get; init; } = "";
        public int Quantity { get; init; }
    }
}
