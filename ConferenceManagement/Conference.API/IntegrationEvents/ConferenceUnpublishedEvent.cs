using Contoso.ServiceBus.Abstractions;

namespace Conference.API.IntegrationEvents;

public record ConferenceUnpublishedEvent : IntegrationEvent
{
    public Guid ConferenceId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
}
