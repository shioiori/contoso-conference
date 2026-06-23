using Eventbox.Shared.Outbox;
using Eventbox.Ticketing.Application.Abstractions.Repositories;

namespace Eventbox.Ticketing.Application.Abstractions;

public interface IUnitOfWork
{
    IOrderRepository Orders { get; }
    ITicketTypeAvailabilityRepository TicketTypeAvailabilities { get; }
    IEventSnapshotRepository EventSnapshots { get; }
    ITicketRepository Tickets { get; }
    IOutbox Outbox { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}
