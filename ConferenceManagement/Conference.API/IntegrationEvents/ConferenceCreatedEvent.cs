using Contoso.ServiceBus.Abstractions;

namespace Conference.API.IntegrationEvents;

public record ConferenceCreatedEvent : IntegrationEvent
{
    public Guid ConferenceId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string AccessCode { get; init; } = "";
}
