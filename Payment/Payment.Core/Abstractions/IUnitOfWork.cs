using Eventbox.Shared.Outbox;

namespace Eventbox.Payment.Core.Abstractions
{
    public interface IUnitOfWork
    {
        IPaymentRepository Payments { get; }
        IOutbox Outbox { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
