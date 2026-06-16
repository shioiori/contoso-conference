using Eventbox.Payment.Core.Abstractions;
using Eventbox.Shared.Outbox;

namespace Eventbox.Payment.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork, IOutbox
    {
        private readonly PaymentDbContext _dbContext;

        public UnitOfWork(PaymentDbContext dbContext)
        {
            _dbContext = dbContext;
            Payments = new PaymentRepository(dbContext);
        }

        public IPaymentRepository Payments { get; }

        public async Task AddOutboxMessageAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
            => await _dbContext.Outboxes.AddAsync(outboxMessage, cancellationToken);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _dbContext.SaveChangesAsync(cancellationToken);

        public Task UpdateOutboxMessageAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
        {
            _dbContext.Outboxes.Update(outboxMessage);
            return Task.CompletedTask;
        }
    }
}
