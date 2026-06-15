using Eventbox.TicketingDomain.Entities.TicketAvailabilityAggregate;

namespace Eventbox.TicketingApplication.Abstractions.Repositories
{
    public interface ITicketAvailabilityRepository : IRepository<TicketAvailability, Guid>
    {
        Task<TicketAvailability?> GetByEventIdAsync(Guid EventId, CancellationToken cancellationToken = default);
        Task<bool> TryReserveAsync(Guid eventId, int ticketTypeId, int quantity, CancellationToken cancellationToken = default);
    }
}
