namespace Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories
{
    public interface IRepository<TEntity, TId>
    {
        Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
