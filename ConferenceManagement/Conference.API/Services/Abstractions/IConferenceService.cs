using Conference.API.Domains;

namespace Conference.API.Services.Abstractions
{
    public interface IConferenceService
    {
        Task<Conference.API.Domains.Conference?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Conference.API.Domains.Conference?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<IEnumerable<Conference.API.Domains.Conference>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Conference.API.Domains.Conference>> GetPublishedAsync(CancellationToken cancellationToken = default);
        Task<Conference.API.Domains.Conference> CreateAsync(string name, string slug, DateTime startDate, DateTime endDate, string? description = null, CancellationToken cancellationToken = default);
        Task<Conference.API.Domains.Conference> UpdateAsync(Guid id, string name, DateTime startDate, DateTime endDate, string? description = null, CancellationToken cancellationToken = default);
        Task SetVisibilityAsync(Guid id, bool isPublished, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
