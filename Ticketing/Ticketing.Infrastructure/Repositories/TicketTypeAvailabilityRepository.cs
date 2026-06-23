using Microsoft.EntityFrameworkCore;
using Eventbox.Ticketing.Domain.Inventory;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Shared.Infrastructure.Repositories;

namespace Eventbox.Ticketing.Infrastructure.Repositories
{
    public class TicketTypeAvailabilityRepository(TicketingDbContext dbContext)
        : BaseRepository<TicketingDbContext, TicketTypeAvailability, Guid>(dbContext), ITicketTypeAvailabilityRepository
    {
        public async Task<TicketTypeAvailability?> GetByIdWithPricingAsync(Guid ticketTypeId, CancellationToken cancellationToken = default)
            => await dbContext.TicketTypeAvailabilities
                .Include(t => t.PricingPhases)
                .FirstOrDefaultAsync(t => t.Id == ticketTypeId, cancellationToken);

        public async Task<IReadOnlyList<TicketTypeAvailability>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
            => await dbContext.TicketTypeAvailabilities
                .Include(t => t.PricingPhases)
                .Where(t => t.EventId == eventId)
                .ToListAsync(cancellationToken);

        public async Task<bool> TryReserveAsync(Guid ticketTypeId, int quantity, CancellationToken cancellationToken = default)
        {
            var affectedRows = await dbContext.TicketTypeAvailabilities
                .Where(tt => tt.Id == ticketTypeId && tt.Remaining >= quantity)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(tt => tt.Remaining, tt => tt.Remaining - quantity),
                    cancellationToken);

            return affectedRows == 1;
        }
    }
}
