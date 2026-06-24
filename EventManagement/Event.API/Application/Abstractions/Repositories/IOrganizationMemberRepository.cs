using Eventbox.EventManagement.EventApi.Domains;

namespace Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories
{
    public interface IOrganizationMemberRepository
    {
        Task AddAsync(OrganizationMember member, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid organizationId, Guid organizerId, CancellationToken cancellationToken = default);
        Task<OrganizationMember> GetAsync(Guid organizationId, Guid organizerId, CancellationToken cancellationToken = default);
        void Delete(OrganizationMember member);
        Task<IReadOnlyList<Organization>> GetOrganizationsByOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default);
    }
}
