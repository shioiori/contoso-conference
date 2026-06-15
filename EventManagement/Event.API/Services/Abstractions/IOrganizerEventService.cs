using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.EventManagement.EventApi.Dtos.OrganizerEvents;
using Eventbox.EventManagement.EventApi.Dtos.PublicEvents;

namespace Eventbox.EventManagement.EventApi.Services.Abstractions
{
    public interface IOrganizerEventService
    {
        Task<OrganizerEventDto?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
        //Task<Eventbox.EventManagement.EventApi.Domains.Event?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<OrganizerEventDto?> GetPublicReadiness(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrganizerEventDto>> SearchAsync(OrganizerEventSearchDto searchDto, CancellationToken cancellationToken = default);
        Task PublishedAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
        Task UnpublishedAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
        Task<OrganizerEventDto> CreateAsync(Guid? organizationId, string name, string slug, DateTimeOffset from, DateTimeOffset to, string? description = null, CancellationToken cancellationToken = default);
        Task<OrganizerEventDto> UpdateAsync(Guid organizationId, Guid id, string name, DateTimeOffset from, DateTimeOffset to, string? description = null, CancellationToken cancellationToken = default);
        //Task SetVisibilityAsync(Guid id, bool isPublished, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    }

    public interface IPublicEventService
    {
        Task<PublicEventDto?> GetBySlugAsync(string slug, string? accessCode = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<PublicEventDto>> SearchAsync(PublicEventSearchDto searchDto, CancellationToken cancellationToken = default);
    }
}
