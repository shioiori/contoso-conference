using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.EventManagement.EventApi.Domains.Enums;
using Eventbox.Shared.Repositories;
using Event = Eventbox.EventManagement.EventApi.Domains.Event;

namespace Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories
{
    public interface IEventRepository : IRepository<Event, Guid>
    {
        Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
        Task<Event> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<Event> GetByOrganizationAsync(Guid organizationId, Guid eventId, bool includeTicketTypes = false, bool asNoTracking = true, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Event>> SearchForOrganizerAsync(Guid organizationId, EventStatus? status, string? query, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Event>> SearchPublishedAsync(EventStatus? status, string? query, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize, CancellationToken cancellationToken = default);
    }
}
