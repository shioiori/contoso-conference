using Eventbox.Payment.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using PaymentEntity = Eventbox.Payment.Core.Entities.Payment;

namespace Eventbox.Payment.Infrastructure.Persistence
{
    public class PaymentRepository(PaymentDbContext dbContext) : IPaymentRepository
    {
        public Task<PaymentEntity?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken)
            => dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        public Task<PaymentEntity?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
            => dbContext.Payments.FirstOrDefaultAsync(
                p => p.IdempotencyKey == idempotencyKey,
                cancellationToken);

        public Task<PaymentEntity?> GetByProviderEventIdAsync(string providerEventId, CancellationToken cancellationToken)
            => dbContext.Payments.FirstOrDefaultAsync(
                p => p.ProviderEventId == providerEventId,
                cancellationToken);

        public async Task AddAsync(PaymentEntity payment, CancellationToken cancellationToken)
            => await dbContext.Payments.AddAsync(payment, cancellationToken);
    }
}
