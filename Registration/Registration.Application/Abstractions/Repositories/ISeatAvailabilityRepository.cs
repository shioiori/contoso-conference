using Eventbox.Registration.Domain.Entities.SeatAvailabilityAggregate;

namespace Eventbox.Registration.Application.Abstractions.Repositories
{
    public interface ISeatAvailabilityRepository : IRepository<SeatAvailability, Guid>
    {
        Task<SeatAvailability?> GetByEventIdAsync(Guid EventId, CancellationToken cancellationToken = default);
        Task<bool> TryReserveAsync(Guid eventId, int ticketTypeId, int quantity, CancellationToken cancellationToken = default);
    }
}
