using Eventbox.Payment.Core.Abstractions;
using Eventbox.Payment.Infrastructure.Repositories;
using Eventbox.Shared.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.Payment.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PaymentDbContext _dbContext;

        public UnitOfWork(PaymentDbContext dbContext)
        {
            _dbContext = dbContext;
            Payments = new PaymentRepository(dbContext);
            Outbox = new OutboxRepository(dbContext);
        }

        public IPaymentRepository Payments { get; }
        public IOutbox Outbox { get; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _dbContext.SaveChangesAsync(cancellationToken);
    }
}
