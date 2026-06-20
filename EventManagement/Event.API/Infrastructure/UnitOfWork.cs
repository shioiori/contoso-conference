using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Infrastructure.Repositories;
using Eventbox.Payment.Infrastructure.Persistence;
using Eventbox.Shared.Outbox;

namespace Eventbox.EventManagement.EventApi.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EventDbContext _dbContext;

        public UnitOfWork(EventDbContext dbContext)
        {
            _dbContext = dbContext;
            Events = new EventRepository(dbContext);
            TicketTypes = new TicketTypeRepository(dbContext);
            Organizations = new OrganizationRepository(dbContext);
            OrganizationMembers = new OrganizationMemberRepository(dbContext);
            Outbox = new OutboxRepository(dbContext);
        }

        public IEventRepository Events { get; }
        public ITicketTypeRepository TicketTypes { get; }
        public IOrganizationRepository Organizations { get; }
        public IOrganizationMemberRepository OrganizationMembers { get; }
        public IOutbox Outbox { get; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _dbContext.SaveChangesAsync(cancellationToken);
    }
}
