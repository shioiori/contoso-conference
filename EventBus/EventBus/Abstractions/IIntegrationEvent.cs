namespace Eventbox.EventBus.Core.Abstractions;

public interface IIntegrationEvent
{
    Guid IntegrationEventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}
