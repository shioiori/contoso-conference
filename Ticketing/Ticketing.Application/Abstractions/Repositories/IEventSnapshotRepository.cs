using Eventbox.Shared.Abstractions;
using Eventbox.Ticketing.Domain.Events;

namespace Eventbox.Ticketing.Application.Abstractions.Repositories;

public interface IEventSnapshotRepository : IRepository<EventSnapshot, Guid>
{
    Task<EventSnapshot?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task UpsertAsync(Guid eventId, DateTimeOffset? from, DateTimeOffset? to, bool? isPublished, CancellationToken cancellationToken = default);
}
