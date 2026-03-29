using Contoso.ServiceBus.Abstractions;

namespace Conference.API.IntegrationEvents;

public record ConferenceUpdatedEvent : IntegrationEvent
{
    public Guid ConferenceId { get; init; }
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
