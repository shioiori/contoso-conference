using Eventbox.TicketingDomain.Entities;

namespace Eventbox.TicketingApplication.Abstractions.Repositories;

public interface IEventScheduleRepository
{
    Task<EventSchedule?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task UpsertAsync(Guid eventId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);
}
