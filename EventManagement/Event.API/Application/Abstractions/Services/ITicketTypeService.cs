using Eventbox.EventManagement.EventApi.Application.Dtos;

namespace Eventbox.EventManagement.EventApi.Application.Abstractions.Services
{
    public interface ITicketTypeService
    {
        Task<TicketTypeDto?> GetByIdAsync(Guid organizationId, Guid eventId, Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TicketTypeDto>> GetByEventIdAsync(Guid organizationId, Guid eventId, CancellationToken cancellationToken = default);
        Task<TicketTypeDto> CreateAsync(Guid organizationId, Guid eventId, TicketTypeInputDto dto, CancellationToken cancellationToken = default);
        Task<TicketTypeDto> UpdateAsync(Guid organizationId, Guid eventId, Guid ticketTypeId, TicketTypeInputDto dto, CancellationToken cancellationToken = default);
        Task<TicketTypeDto> AddCapacityAsync(Guid organizationId, Guid eventId, Guid ticketTypeId, int quantity, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
