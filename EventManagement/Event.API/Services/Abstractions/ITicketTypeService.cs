using Eventbox.EventManagement.EventApi.Domains;

namespace Eventbox.EventManagement.EventApi.Services.Abstractions
{
    public interface ITicketTypeService
    {
        Task<TicketType?> GetByIdAsync(Guid organizationId, Guid eventId, int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TicketType>> GetByEventIdAsync(Guid organizationId, Guid eventId, CancellationToken cancellationToken = default);
        Task<TicketType> CreateAsync(Guid organizationId, TicketType ticketType, CancellationToken cancellationToken = default);
        Task<TicketType> AddCapacityAsync(Guid organizationId, Guid eventId, int ticketTypeId, int quantity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
