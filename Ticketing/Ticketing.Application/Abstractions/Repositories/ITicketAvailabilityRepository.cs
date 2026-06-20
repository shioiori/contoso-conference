using Eventbox.Ticketing.Domain.Entities.TicketAvailabilityAggregate;

namespace Eventbox.Ticketing.Application.Abstractions.Repositories
{
    public interface ITicketAvailabilityRepository : IRepository<TicketAvailability, Guid>
    {
        Task<TicketAvailability?> GetByEventIdAsync(Guid EventId, CancellationToken cancellationToken = default);
        Task<bool> TryReserveAsync(Guid eventId, Guid ticketTypeId, int quantity, CancellationToken cancellationToken = default);
    }
}
