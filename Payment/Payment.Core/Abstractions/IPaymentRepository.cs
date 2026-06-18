using PaymentEntity = Eventbox.Payment.Core.Entities.Payment;

namespace Eventbox.Payment.Core.Abstractions
{
    public interface IPaymentRepository
    {
        Task<PaymentEntity?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken);

        Task<PaymentEntity?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);

        Task<PaymentEntity?> GetByProviderEventIdAsync(string providerEventId, CancellationToken cancellationToken);

        Task AddAsync(PaymentEntity payment, CancellationToken cancellationToken);
    }
}
