using Conference.API.Domains.Common;

namespace Conference.API.Infrastructure.Repositories.Abstractions
{
    public interface IRepository<TEntity, TId> where TEntity : Entity<TId>
    {
        Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
