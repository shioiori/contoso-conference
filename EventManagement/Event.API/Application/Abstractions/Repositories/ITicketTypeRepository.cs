using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.Shared.Repositories;

namespace Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories
{
    public interface ITicketTypeRepository : IRepository<TicketType, Guid>
    {
        Task<IEnumerable<TicketType>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    }
}
