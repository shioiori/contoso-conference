using Eventbox.Shared.Outbox;
using Eventbox.Shared.SeedWorks;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Infrastructure.Repositories;
using Eventbox.Ticketing.Infrastructure.Tickets;

namespace Eventbox.Ticketing.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly TicketingDbContext _dbContext;

    public UnitOfWork(TicketingDbContext dbContext)
    {
        _dbContext = dbContext;
        Orders = new OrderRepository(dbContext);
        Tickets = new TicketRepository(dbContext);
        TicketAvailabilities = new TicketAvailabilityRepository(dbContext);
        EventSnapshots = new EventSnapshotRepository(dbContext);
        Outbox = new OutboxRepository(dbContext);
    }

    public IOrderRepository Orders { get; }
    public ITicketRepository Tickets { get; }
    public ITicketAvailabilityRepository TicketAvailabilities { get; }
    public IEventSnapshotRepository EventSnapshots { get; }
    public IOutbox Outbox { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in _dbContext.ChangeTracker.Entries<IAuditableEntity>())
        {
            foreach (var prop in entry.Properties)
            {
                if (prop.Metadata.IsConcurrencyToken)
                    Console.WriteLine($"{entry.Entity.GetType().Name}.{prop.Metadata.Name} is concurrency token");
            }
        }
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await operation();

        await transaction.CommitAsync(cancellationToken);
    }
}
