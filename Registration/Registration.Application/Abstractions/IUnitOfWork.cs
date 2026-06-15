using Eventbox.Registration.Application.Abstractions.Repositories;

namespace Eventbox.Registration.Application.Abstractions;

public interface IUnitOfWork
{
    IOrderRepository Orders { get; }
    ISeatAvailabilityRepository SeatAvailabilities { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}
