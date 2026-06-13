using Eventbox.EventManagement.EventApi.Domains;

namespace Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories
{
    public interface ISeatTypeRepository : IRepository<SeatType, int>
    {
        Task<IEnumerable<SeatType>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    }
}
