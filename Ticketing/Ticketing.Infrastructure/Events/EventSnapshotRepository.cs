using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.Ticketing.Infrastructure.Repositories;

public class EventSnapshotRepository(TicketingDbContext dbContext) : IEventSnapshotRepository
{
    public Task<EventSnapshot?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        => dbContext.EventSnapshots.FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

    public async Task UpsertAsync(Guid eventId, DateTimeOffset? from, DateTimeOffset? to, bool? isPublished, CancellationToken cancellationToken = default)
    {
        var snapshot = await GetByEventIdAsync(eventId, cancellationToken);
        if (from.HasValue && to.HasValue)
        {
            if (snapshot == null)
            {
                await dbContext.EventSnapshots.AddAsync(new EventSnapshot(eventId, from.Value, to.Value, isPublished ?? false), cancellationToken);
                return;
            }
            else snapshot.Update(from.Value, to.Value, isPublished);
        }
        else if (isPublished.HasValue && snapshot != null)
        {
            if (isPublished.Value) snapshot.Publish();
            else snapshot.Unpublish();
        }

        if (snapshot != null)
            dbContext.EventSnapshots.Update(snapshot);
    }
}
