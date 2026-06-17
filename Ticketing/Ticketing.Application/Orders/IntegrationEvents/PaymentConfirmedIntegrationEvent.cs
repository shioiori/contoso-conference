using Eventbox.EventBus.Core.Abstractions;

namespace Eventbox.Ticketing.Application.IntegrationEvents;

public sealed class PaymentConfirmedIntegrationEvent : IIntegrationEvent
{
    public PaymentConfirmedIntegrationEvent(
        Guid paymentId,
        string providerEventId,
        Guid orderId,
        decimal amount,
        string currency,
        DateTimeOffset paidAt)
    {
        PaymentId = paymentId;
        ProviderEventId = providerEventId;
        OrderId = orderId;
        Amount = amount;
        Currency = currency;
        PaidAt = paidAt;
    }

    public Guid PaymentId { get; }
    public string ProviderEventId { get; }
    public Guid OrderId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public DateTimeOffset PaidAt { get; }
    public Guid IntegrationEventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
