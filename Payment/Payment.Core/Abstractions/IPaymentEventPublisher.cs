using PaymentEntity = Eventbox.Payment.Core.Entities.Payment;

namespace Eventbox.Payment.Core.Abstractions
{
    public interface IPaymentEventPublisher
    {
        Task PublishPaymentConfirmedAsync(
            PaymentEntity payment,
            string providerEventId,
            DateTimeOffset paidAt,
            CancellationToken cancellationToken);
    }
}
