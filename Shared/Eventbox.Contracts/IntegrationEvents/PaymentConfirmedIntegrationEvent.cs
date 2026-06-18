using Eventbox.EventBus.Events;

namespace Eventbox.Contracts.IntegrationEvents;

public sealed class PaymentConfirmedIntegrationEvent : IntegrationEvent
{
    public Guid PaymentId { get; set; }
    public string ProviderEventId { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTimeOffset PaidAt { get; set; }
}
