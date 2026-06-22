namespace Eventbox.Ticketing.Application.Dtos.Orders
{
    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid TicketTypeId { get; set; }
        public Guid PricingPhaseId { get; set; }
        public string TicketTypeName { get; set; }
        public string PricingPhaseName { get; set; }
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; } 
        public int Quantity { get; set; }
    }
}
