using Eventbox.EventManagement.EventApi.Application.Dtos;

namespace Eventbox.EventManagement.EventApi.Application.Abstractions.Services
{
    public interface IOrganizationService
    {
        Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<OrganizationDto> CreateAsync(string name, CancellationToken cancellationToken = default);
        Task<OrganizationDto> UpdateAsync(Guid id, string name, CancellationToken cancellationToken = default);
        Task Delete(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<OrganizationDto>> GetMyOrganizationsAsync(CancellationToken cancellationToken = default);
        Task AddOrganizerAsync(Guid organizationId, Guid organizerId, CancellationToken cancellationToken = default);
        Task RemoveOrganizerAsync(Guid organizationId, Guid organizerId, CancellationToken cancellationToken = default);
        Task<bool> IsMemberAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default);
    }
}
