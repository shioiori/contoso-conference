using Microsoft.EntityFrameworkCore;
using Eventbox.Ticketing.Domain.Entities.TicketAvailabilityAggregate;
using Eventbox.Ticketing.Infrastructure.Repositories.Common;
using Eventbox.Ticketing.Application.Abstractions.Repositories;

namespace Eventbox.Ticketing.Infrastructure.Repositories
{
    public class TicketAvailabilityRepository(TicketingDbContext dbContext) : BaseRepository<TicketingDbContext, TicketAvailability, Guid>(dbContext), ITicketAvailabilityRepository
    {
        public async Task<TicketAvailability?> GetByEventIdAsync(Guid EventId, CancellationToken cancellationToken = default)
            => await dbContext.TicketAvailabilities
                .Include(s => s.TicketTypes)
                    .ThenInclude(t => t.PricingPhases)
                .FirstOrDefaultAsync(s => s.Id == EventId, cancellationToken);

        public async Task<bool> TryReserveAsync(Guid eventId, Guid ticketTypeId, int quantity, CancellationToken cancellationToken = default)
        {
            var affectedRows = await dbContext.Set<TicketTypeAvailability>()
                .Where(ticketType =>
                    EF.Property<Guid>(ticketType, "TicketAvailabilityId") == eventId &&
                    ticketType.Id == ticketTypeId &&
                    ticketType.Remaining >= quantity)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(ticketType => ticketType.Remaining, ticketType => ticketType.Remaining - quantity),
                    cancellationToken);

            return affectedRows == 1;
        }
    }
}
