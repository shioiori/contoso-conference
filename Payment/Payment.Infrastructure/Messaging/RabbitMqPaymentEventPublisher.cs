using Eventbox.EventBus.Core.Abstractions;
using Eventbox.EventBus.Events;
using Eventbox.Payment.Core.Abstractions;
using PaymentEntity = Eventbox.Payment.Core.Entities.Payment;

namespace Eventbox.Payment.Infrastructure.Messaging
{
    public sealed class PaymentConfirmedIntegrationEvent : IntegrationEvent
    {
        public Guid PaymentId { get; set; }
        public string ProviderEventId { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTimeOffset PaidAt { get; set; }
    }

    public class RabbitMqPaymentEventPublisher(IEventBus eventBus) : IPaymentEventPublisher
    {
        public Task PublishPaymentConfirmedAsync(
            PaymentEntity payment,
            string providerEventId,
            DateTimeOffset paidAt,
            CancellationToken cancellationToken)
            => eventBus.PublishAsync(
                new PaymentConfirmedIntegrationEvent()
                {
                    PaymentId = payment.Id,
                    ProviderEventId = providerEventId,
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    PaidAt = paidAt
                },
                cancellationToken);
    }
}
