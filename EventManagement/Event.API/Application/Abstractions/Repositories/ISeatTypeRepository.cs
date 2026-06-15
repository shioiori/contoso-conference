using Eventbox.EventManagement.EventApi.Domains;

namespace Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories
{
    public interface ITicketTypeRepository : IRepository<TicketType, int>
    {
        Task<IEnumerable<TicketType>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    }
}
