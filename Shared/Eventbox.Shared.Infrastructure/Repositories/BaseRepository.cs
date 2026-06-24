using Eventbox.Shared.Abstractions;
using Eventbox.Shared.SeedWorks;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Eventbox.Shared.Infrastructure.Repositories
{
    public class BaseRepository<TContext, TEntity, TPrimarykey> : IRepository<TEntity, TPrimarykey>
        where TEntity : Entity<TPrimarykey>
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

        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null, bool needAsNoTracking = true)
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
                query = query.Where(filter);

            if (!string.IsNullOrEmpty(includeProperties))
                foreach (var includeProperty in includeProperties.Split
                         (',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }

            if (orderBy != null)
                query = orderBy(query);

            return needAsNoTracking ? query.AsNoTracking() : query;
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
