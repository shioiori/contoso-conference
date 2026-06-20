using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.Ticketing.Infrastructure.Repositories;

public class EventScheduleRepository(TicketingDbContext dbContext) : IEventScheduleRepository
{
    public Task<EventSnapshot?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        => dbContext.EventSchedules.FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

    public async Task UpsertAsync(Guid eventId, DateTimeOffset? from, DateTimeOffset? to, bool? isPublished, CancellationToken cancellationToken = default)
    {
        var schedule = await GetByEventIdAsync(eventId, cancellationToken);
        if (from.HasValue && to.HasValue)
        {
            if (schedule == null)
            {
                await dbContext.EventSchedules.AddAsync(new EventSnapshot(eventId, from.Value, to.Value, isPublished ?? false), cancellationToken);
                return;
            }
            else schedule.Update(from.Value, to.Value, isPublished);
        }
        else if (isPublished.HasValue && schedule != null)
        {
            if (isPublished.Value) schedule.Publish();
            else schedule.Unpublish();
        }

        if (schedule != null)
            dbContext.EventSchedules.Update(schedule);
    }
}
