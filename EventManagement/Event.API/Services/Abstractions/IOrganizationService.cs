using Eventbox.EventManagement.EventApi.Dtos;

namespace Eventbox.EventManagement.EventApi.Services.Abstractions
{
    public interface IOrganizationService
    {
        Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<OrganizationDto> Create(OrganizationDto organizationDto, CancellationToken cancellationToken = default);
        Task<OrganizationDto> Update(Guid id, OrganizationDto organizationDto, CancellationToken cancellationToken = default);
        Task Delete(Guid id, CancellationToken cancellationToken = default);
    }
}
