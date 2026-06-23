namespace Eventbox.Ticketing.Application.Dtos.Inventory
{
    public class TicketTypeAvailabilityDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Remaining { get; set; }
        public string Currency { get; set; }
        public int MinPerOrder { get; set; }
        public int? MaxPerOrder { get; set; }
        public string Visibility { get; set; }
        public IReadOnlyCollection<PricingPhaseAvailabilityDto> PricingPhases { get; set; }
    }

    public class PricingPhaseAvailabilityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
    }
}
