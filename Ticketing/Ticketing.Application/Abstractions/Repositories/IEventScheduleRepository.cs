using Eventbox.Ticketing.Domain.Entities;

namespace Eventbox.Ticketing.Application.Abstractions.Repositories;

public interface IEventScheduleRepository
{
    Task<EventSchedule?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task UpsertAsync(Guid eventId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);
}
