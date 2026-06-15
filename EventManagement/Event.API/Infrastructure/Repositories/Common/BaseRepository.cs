using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.EventManagement.EventApi.Infrastructure.Repositories.Common
{
    public class BaseRepository<TContext, TEntity, TPrimarykey> : IRepository<TEntity, TPrimarykey>
        where TEntity : class
        where TContext : DbContext
    {
        private readonly DbSet<TEntity> _dbSet;
        protected readonly TContext DbContext;

        public BaseRepository(TContext dbContext)
        {
            DbContext = dbContext;
            _dbSet = DbContext.Set<TEntity>();
        }

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<TEntity?> GetByIdAsync(TPrimarykey id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }
    }
}
