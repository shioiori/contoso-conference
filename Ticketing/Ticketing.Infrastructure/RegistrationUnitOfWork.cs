using Eventbox.TicketingApplication.Abstractions;

namespace Eventbox.TicketingInfrastructure;

public class RegistrationUnitOfWork(RegistrationDbContext dbContext) : IRegistrationUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await operation();

        await transaction.CommitAsync(cancellationToken);
    }
}
