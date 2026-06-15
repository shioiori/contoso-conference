using Eventbox.EventManagement.EventApi.Domains;

namespace Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories
{
    public interface IEventRepository : IRepository<Domains.Event, Guid>
    {
        Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
        Task<Domains.Event?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<Domains.Event?> GetByOrganizationAsync(Guid organizationId, Guid eventId, bool includeTicketTypes = false, bool asNoTracking = true, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Domains.Event>> SearchForOrganizerAsync(Guid organizationId, EventStatus? status, string? query, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Domains.Event>> SearchPublishedAsync(EventStatus? status, string? query, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize, CancellationToken cancellationToken = default);
    }
}
