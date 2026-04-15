namespace Conference.API.Infrastructure.Repositories.Abstractions
{
    public interface IConferenceRepository : IRepository<Domains.Conference, Guid>
    {
        Task<Domains.Conference?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<IEnumerable<Domains.Conference>> GetPublishedAsync(CancellationToken cancellationToken = default);
    }
}
