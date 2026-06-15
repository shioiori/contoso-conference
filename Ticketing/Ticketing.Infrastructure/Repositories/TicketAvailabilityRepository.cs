using Microsoft.EntityFrameworkCore;
using Eventbox.TicketingDomain.Entities.TicketAvailabilityAggregate;
using Eventbox.TicketingInfrastructure.Repositories.Common;
using Eventbox.TicketingApplication.Abstractions.Repositories;

namespace Eventbox.TicketingInfrastructure.Repositories
{
    public class TicketAvailabilityRepository(RegistrationDbContext dbContext) : BaseRepository<RegistrationDbContext, TicketAvailability, Guid>(dbContext), ITicketAvailabilityRepository
    {
        public async Task<TicketAvailability?> GetByEventIdAsync(Guid EventId, CancellationToken cancellationToken = default)
            => await dbContext.TicketAvailabilities
                .Include(s => s.TicketTypes)
                    .ThenInclude(t => t.PricingPhases)
                .FirstOrDefaultAsync(s => s.Id == EventId, cancellationToken);

        public async Task<bool> TryReserveAsync(Guid eventId, int ticketTypeId, int quantity, CancellationToken cancellationToken = default)
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
