using Eventbox.Ticketing.Application.Abstractions.Repositories;

namespace Eventbox.Ticketing.Application.Abstractions;

public interface IUnitOfWork
{
    IOrderRepository Orders { get; }
    ITicketAvailabilityRepository TicketAvailabilities { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}
