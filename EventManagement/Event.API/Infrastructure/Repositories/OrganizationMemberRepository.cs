using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Domains;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.EventManagement.EventApi.Infrastructure.Repositories
{
    public class OrganizationMemberRepository(EventDbContext dbContext) : IOrganizationMemberRepository
    {
        public async Task AddAsync(OrganizationMember member, CancellationToken cancellationToken = default)
            => await dbContext.OrganizationMembers.AddAsync(member, cancellationToken);

        public Task<bool> ExistsAsync(Guid organizationId, Guid organizerId, CancellationToken cancellationToken = default)
            => dbContext.OrganizationMembers.AnyAsync(
                m => m.OrganizationId == organizationId && m.OrganizerId == organizerId,
                cancellationToken);

        public Task<OrganizationMember?> GetAsync(Guid organizationId, Guid organizerId, CancellationToken cancellationToken = default)
            => dbContext.OrganizationMembers.FirstOrDefaultAsync(
                m => m.OrganizationId == organizationId && m.OrganizerId == organizerId,
                cancellationToken);

        public void Delete(OrganizationMember member)
            => dbContext.OrganizationMembers.Remove(member);

        public async Task<IReadOnlyList<Organization>> GetOrganizationsByOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default)
            => await dbContext.OrganizationMembers
                .Where(m => m.OrganizerId == organizerId)
                .Join(dbContext.Organizations, m => m.OrganizationId, o => o.Id, (_, o) => o)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
    }
}
