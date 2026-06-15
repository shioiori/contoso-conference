using Eventbox.TicketingApplication.Abstractions.Repositories;
using Eventbox.TicketingDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.TicketingInfrastructure.Repositories;

public class EventScheduleRepository(RegistrationDbContext dbContext) : IEventScheduleRepository
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
