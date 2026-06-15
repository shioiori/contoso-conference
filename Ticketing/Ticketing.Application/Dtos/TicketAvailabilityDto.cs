namespace Eventbox.TicketingApplication.Dtos
{
    public class TicketAvailabilityDto
    {
        public TicketAvailabilityDto(Guid eventId, IReadOnlyCollection<TicketTypeAvailabilityDto> ticketTypes)
        {
            EventId = eventId;
            TicketTypes = ticketTypes;
        }

        public Guid EventId { get; }
        public IReadOnlyCollection<TicketTypeAvailabilityDto> TicketTypes { get; }
    }

    public class TicketTypeAvailabilityDto
    {
        public TicketTypeAvailabilityDto(
            int id,
            string name,
            int quantity,
            int remaining,
            string currency,
            int minPerOrder,
            int? maxPerOrder,
            string visibility,
            IReadOnlyCollection<PricingPhaseAvailabilityDto> pricingPhases)
        {
            Id = id;
            Name = name;
            Quantity = quantity;
            Remaining = remaining;
            Currency = currency;
            MinPerOrder = minPerOrder;
            MaxPerOrder = maxPerOrder;
            Visibility = visibility;
            PricingPhases = pricingPhases;
        }

        public int Id { get; }
        public string Name { get; }
        public int Quantity { get; }
        public int Remaining { get; }
        public string Currency { get; }
        public int MinPerOrder { get; }
        public int? MaxPerOrder { get; }
        public string Visibility { get; }
        public IReadOnlyCollection<PricingPhaseAvailabilityDto> PricingPhases { get; }
    }

    public class PricingPhaseAvailabilityDto
    {
        public PricingPhaseAvailabilityDto(int id, string name, decimal price, DateTimeOffset? startTime, DateTimeOffset? endTime)
        {
            Id = id;
            Name = name;
            Price = price;
            StartTime = startTime;
            EndTime = endTime;
        }

        public int Id { get; }
        public string Name { get; }
        public decimal Price { get; }
        public DateTimeOffset? StartTime { get; }
        public DateTimeOffset? EndTime { get; }
    }
}
