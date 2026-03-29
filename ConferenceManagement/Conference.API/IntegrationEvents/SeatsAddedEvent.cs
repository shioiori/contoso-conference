using Contoso.ServiceBus.Abstractions;

namespace Conference.API.IntegrationEvents;

public record SeatsAddedEvent : IntegrationEvent
{
    public int SeatTypeId { get; init; }
    public Guid ConferenceId { get; init; }
    public int PreviousQuota { get; init; }
    public int NewQuota { get; init; }
    public int AddedQuantity { get; init; }
}
