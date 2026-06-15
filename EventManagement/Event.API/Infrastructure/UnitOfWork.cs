using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Infrastructure.Repositories;

namespace Eventbox.EventManagement.EventApi.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EventDbContext _dbContext;

        public UnitOfWork(EventDbContext dbContext)
        {
            _dbContext = dbContext;
            Events = new EventRepository(dbContext);
            SeatTypes = new SeatTypeRepository(dbContext);
            Organizations = new OrganizationRepository(dbContext);
        }

        public IEventRepository Events { get; }
        public ISeatTypeRepository SeatTypes { get; }
        public IOrganizationRepository Organizations { get; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _dbContext.SaveChangesAsync(cancellationToken);
    }
}
