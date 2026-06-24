using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.Shared.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.EventManagement.EventApi.Infrastructure.Repositories
{
    public class TicketTypeRepository(EventDbContext dbContext) 
        : BaseRepository<EventDbContext, TicketType, Guid>(dbContext), ITicketTypeRepository
    {
        public new async Task<TicketType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await dbContext.TicketTypes
                .Include(s => s.PricingPhases)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        public async Task<IEnumerable<TicketType>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
            => await dbContext.TicketTypes
                .Include(s => s.PricingPhases)
                .Where(s => s.EventId == eventId)
                .ToListAsync(cancellationToken);

    }
}
