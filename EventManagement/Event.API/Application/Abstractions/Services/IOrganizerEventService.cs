using Eventbox.EventManagement.EventApi.Api.Requests;
using Eventbox.EventManagement.EventApi.Application.Dtos.OrganizerEvents;

namespace Eventbox.EventManagement.EventApi.Application.Abstractions.Services
{
    public interface IOrganizerEventService
    {
        Task<OrganizerEventDto?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
        Task<OrganizerEventDto?> GetPublicReadiness(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrganizerEventDto>> SearchAsync(OrganizerEventSearchRequest searchDto, CancellationToken cancellationToken = default);
        Task PublishedAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
        Task UnpublishedAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
        Task<OrganizerEventDto> CreateAsync(Guid organizationId, CreateEventDto dto, CancellationToken cancellationToken = default);
        Task<OrganizerEventDto> UpdateAsync(Guid organizationId, Guid id, UpdateEventDto dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    }
}
