using Eventbox.EventBus.Events;

namespace Eventbox.Contracts.IntegrationEvents;

public class PaymentFailedIntegrationEvent : IntegrationEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public string? FailureReason { get; set; }
}
