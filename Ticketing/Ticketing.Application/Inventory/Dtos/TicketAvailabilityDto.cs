namespace Eventbox.Ticketing.Application.Dtos
{
    public class TicketAvailabilityDto
    {
        public Guid EventId { get; init; }
        public IReadOnlyCollection<TicketTypeAvailabilityDto> TicketTypes { get; init; } = [];
    }

    public class TicketTypeAvailabilityDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public int Remaining { get; init; }
        public string Currency { get; init; } = string.Empty;
        public int MinPerOrder { get; init; }
        public int? MaxPerOrder { get; init; }
        public string Visibility { get; init; } = string.Empty;
        public IReadOnlyCollection<PricingPhaseAvailabilityDto> PricingPhases { get; init; } = [];
    }

    public class PricingPhaseAvailabilityDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public DateTimeOffset? StartTime { get; init; }
        public DateTimeOffset? EndTime { get; init; }
    }
}
