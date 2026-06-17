using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.Ticketing.Infrastructure.Repositories;

public class EventScheduleRepository(TicketingDbContext dbContext) : IEventScheduleRepository
{
    public Task<EventSchedule?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        => dbContext.EventSchedules.FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

    public async Task UpsertAsync(Guid eventId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default)
    {
        var schedule = await GetByEventIdAsync(eventId, cancellationToken);
        if (schedule is null)
        {
            await dbContext.EventSchedules.AddAsync(new EventSchedule(eventId, from, to), cancellationToken);
            return;
        }

        schedule.Update(from, to);
        dbContext.EventSchedules.Update(schedule);
    }
}
