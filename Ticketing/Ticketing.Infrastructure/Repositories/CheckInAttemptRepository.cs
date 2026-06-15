using Eventbox.TicketingApplication.Abstractions.Repositories;
using Eventbox.TicketingDomain.Entities.CheckInAggregate;

namespace Eventbox.TicketingInfrastructure.Repositories;

public class CheckInAttemptRepository(RegistrationDbContext dbContext) : ICheckInAttemptRepository
{
    public async Task AddAsync(CheckInAttempt attempt, CancellationToken cancellationToken = default)
        => await dbContext.CheckInAttempts.AddAsync(attempt, cancellationToken);
}
