using Eventbox.Shared.Repositories;
using Eventbox.Ticketing.Domain.Inventory;

namespace Eventbox.Ticketing.Application.Abstractions.Repositories
{
    public interface ITicketTypeAvailabilityRepository : IRepository<TicketTypeAvailability, Guid>
    {
        Task<TicketTypeAvailability?> GetByIdWithPricingAsync(Guid ticketTypeId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TicketTypeAvailability>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
        Task<bool> TryReserveAsync(Guid ticketTypeId, int quantity, CancellationToken cancellationToken = default);
    }
}
