using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.EventManagement.EventApi.Infrastructure;
using Eventbox.EventManagement.EventApi.Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.EventManagement.EventApi.Infrastructure.Repositories
{
    public class SeatTypeRepository(EventDbContext dbContext) : BaseRepository<EventDbContext, SeatType, int>(dbContext), ISeatTypeRepository
    {
        public async Task<IEnumerable<SeatType>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
            => await dbContext.SeatTypes.Where(s => s.EventId == eventId).ToListAsync(cancellationToken);

    }
}
