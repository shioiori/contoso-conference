namespace Eventbox.Payment.Core.Abstractions
{
    public interface IUnitOfWork
    {
        IPaymentRepository Payments { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
