using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Infrastructure.Repositories;

namespace Eventbox.Ticketing.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly TicketingDbContext _dbContext;

    public UnitOfWork(TicketingDbContext dbContext)
    {
        _dbContext = dbContext;
        Orders = new OrderRepository(dbContext);
        TicketAvailabilities = new TicketAvailabilityRepository(dbContext);
    }

    public IOrderRepository Orders { get; }
    public ITicketAvailabilityRepository TicketAvailabilities { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await operation();

        await transaction.CommitAsync(cancellationToken);
    }
}
