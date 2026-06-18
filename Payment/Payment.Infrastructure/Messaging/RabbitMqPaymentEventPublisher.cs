using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Payment.Core.Abstractions;
using PaymentEntity = Eventbox.Payment.Core.Entities.Payment;

namespace Eventbox.Payment.Infrastructure.Messaging
{
    public class RabbitMqPaymentEventPublisher(IEventBus eventBus) : IPaymentEventPublisher
    {
        public async Task<bool> PublishPaymentConfirmedAsync(
            PaymentEntity payment,
            string providerEventId,
            DateTimeOffset paidAt,
            CancellationToken cancellationToken)
        {
            try
            {
                await eventBus.PublishAsync(
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

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
