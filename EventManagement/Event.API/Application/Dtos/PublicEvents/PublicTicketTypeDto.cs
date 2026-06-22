using Eventbox.EventManagement.EventApi.Domains.Enums;

namespace Eventbox.EventManagement.EventApi.Application.Dtos.PublicEvents;

public class PublicTicketTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int Quota { get; set; }
    public string Currency { get; set; }
    public int MinPerOrder { get; set; }
    public int? MaxPerOrder { get; set; }
    public TicketVisibility Visibility { get; set; }
    public IEnumerable<PublicPricingPhaseDto> PricingPhases { get; set; }
}

public class PublicPricingPhaseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } 
    public decimal Price { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
}
