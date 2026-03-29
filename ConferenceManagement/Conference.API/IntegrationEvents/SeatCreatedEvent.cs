using Contoso.ServiceBus.Abstractions;

namespace Conference.API.IntegrationEvents;

public record SeatCreatedEvent : IntegrationEvent
{
    public int SeatTypeId { get; init; }
    public Guid ConferenceId { get; init; }
    public string Name { get; init; } = "";
    public int Quota { get; init; }
}
