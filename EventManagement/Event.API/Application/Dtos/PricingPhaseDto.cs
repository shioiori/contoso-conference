namespace Eventbox.EventManagement.EventApi.Application.Dtos;

public class PricingPhaseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
}
