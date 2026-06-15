using Eventbox.EventBus.Events;

namespace Eventbox.Ticketing.Application.IntegrationEvents;

public class TicketTypeCreatedEvent : IntegrationEvent
{
    public int Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public string Currency { get; set; } = "VND";
    public int MinPerOrder { get; set; } = 1;
    public int? MaxPerOrder { get; set; }
    public string Visibility { get; set; } = "Public";
    public string? AccessCodeHash { get; set; }
    public List<TicketTypePricingPhaseSnapshot> PricingPhases { get; set; } = [];
}

public class TicketTypePricingPhaseSnapshot
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
}
