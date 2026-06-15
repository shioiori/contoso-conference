using Eventbox.TicketingDomain.Entities.CheckInAggregate;

namespace Eventbox.TicketingApplication.Abstractions.Repositories;

public interface ICheckInAttemptRepository
{
    Task AddAsync(CheckInAttempt attempt, CancellationToken cancellationToken = default);
}
