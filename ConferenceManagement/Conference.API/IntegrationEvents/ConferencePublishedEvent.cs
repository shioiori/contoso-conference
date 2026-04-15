using Contoso.EventBus.Abstractions;
using Contoso.EventBus.Events;

namespace Conference.API.IntegrationEvents;

public record ConferencePublishedEvent : IntegrationEvent
{
    public Guid ConferenceId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
}
