using Microsoft.EntityFrameworkCore;
using Eventbox.Registration.Domain.Entities.SeatAvailabilityAggregate;
using Eventbox.Registration.Infrastructure.Repositories.Common;
using Eventbox.Registration.Application.Abstractions.Repositories;

namespace Eventbox.Registration.Infrastructure.Repositories
{
    public class SeatAvailabilityRepository(RegistrationDbContext dbContext) : BaseRepository<RegistrationDbContext, SeatAvailability, Guid>(dbContext), ISeatAvailabilityRepository
    {
        public async Task<SeatAvailability?> GetByEventIdAsync(Guid EventId, CancellationToken cancellationToken = default)
            => await dbContext.SeatAvailabilities
                .Include(s => s.TicketTypes)
                .FirstOrDefaultAsync(s => s.Id == EventId, cancellationToken);

        public async Task<bool> TryReserveAsync(Guid eventId, int ticketTypeId, int quantity, CancellationToken cancellationToken = default)
        {
            var affectedRows = await dbContext.Set<TicketTypeAvailability>()
                .Where(ticketType =>
                    EF.Property<Guid>(ticketType, "SeatAvailabilityId") == eventId &&
                    ticketType.TicketTypeId == ticketTypeId &&
                    ticketType.Remaining >= quantity)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(ticketType => ticketType.Remaining, ticketType => ticketType.Remaining - quantity),
                    cancellationToken);

            return affectedRows == 1;
        }
    }
}
