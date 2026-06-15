using Eventbox.Registration.Application.Abstractions;
using Eventbox.Registration.Application.Abstractions.Repositories;
using Eventbox.Registration.Infrastructure.Repositories;

namespace Eventbox.Registration.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly RegistrationDbContext _dbContext;

    public UnitOfWork(RegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
        Orders = new OrderRepository(dbContext);
        SeatAvailabilities = new SeatAvailabilityRepository(dbContext);
    }

    public IOrderRepository Orders { get; }
    public ISeatAvailabilityRepository SeatAvailabilities { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await operation();

        await transaction.CommitAsync(cancellationToken);
    }
}
