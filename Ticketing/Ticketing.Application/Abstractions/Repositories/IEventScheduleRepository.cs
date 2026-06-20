using Eventbox.Ticketing.Domain.Entities;

namespace Eventbox.Ticketing.Application.Abstractions.Repositories;

public interface IEventScheduleRepository
{
    Task<EventSnapshot?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task UpsertAsync(Guid eventId, DateTimeOffset? from, DateTimeOffset? to, bool? isPublished, CancellationToken cancellationToken = default);
}
