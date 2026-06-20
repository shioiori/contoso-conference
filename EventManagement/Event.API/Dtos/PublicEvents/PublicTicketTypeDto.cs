using Eventbox.EventManagement.EventApi.Enums;

namespace Eventbox.EventManagement.EventApi.Dtos.PublicEvents;

public class PublicTicketTypeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public int Quota { get; init; }
    public string Currency { get; init; } = "VND";
    public int MinPerOrder { get; init; }
    public int? MaxPerOrder { get; init; }
    public TicketVisibility Visibility { get; init; }
    public IEnumerable<PublicPricingPhaseDto> PricingPhases { get; init; } = [];
}

public class PublicPricingPhaseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public decimal Price { get; init; }
    public DateTimeOffset? StartTime { get; init; }
    public DateTimeOffset? EndTime { get; init; }
}
