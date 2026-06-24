using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.Shared.Outbox;

namespace Eventbox.EventManagement.EventApi.Infrastructure
{
    public class UnitOfWork(
        EventDbContext dbContext,
        IEventRepository events,
        ITicketTypeRepository ticketTypes,
        IOrganizationRepository organizations,
        IOrganizationMemberRepository organizationMembers,
        IOutbox outbox) : IUnitOfWork
    {
        public IEventRepository Events { get; } = events;
        public ITicketTypeRepository TicketTypes { get; } = ticketTypes;
        public IOrganizationRepository Organizations { get; } = organizations;
        public IOrganizationMemberRepository OrganizationMembers { get; } = organizationMembers;
        public IOutbox Outbox { get; } = outbox;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => dbContext.SaveChangesAsync(cancellationToken);
    }
}
