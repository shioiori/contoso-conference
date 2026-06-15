using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.EventManagement.EventApi.Dtos;
using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Eventbox.Shared.Exceptions;
using Mapster;

namespace Eventbox.EventManagement.EventApi.Services
{
    public class OrganizationService(IOrganizationRepository organizationRepository) : IOrganizationService
    {
        public async Task<OrganizationDto> Create(OrganizationDto organizationDto, CancellationToken cancellationToken = default)
        {
            var id = organizationDto.Id == Guid.Empty ? Guid.NewGuid() : organizationDto.Id;
            var organization = new Organization(id, organizationDto.Name);

            await organizationRepository.AddAsync(organization, cancellationToken);
            await organizationRepository.SaveChangesAsync(cancellationToken);

            return organization.Adapt<OrganizationDto>();
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var organization = await organizationRepository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Organization", id);

            organizationRepository.Delete(organization);
            await organizationRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return (await organizationRepository.GetByIdAsync(id, cancellationToken))?.Adapt<OrganizationDto>();
        }

        public async Task<OrganizationDto> Update(Guid id, OrganizationDto organizationDto, CancellationToken cancellationToken = default)
        {
            var organization = await organizationRepository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Organization", id);

            organization.Update(organizationDto.Name);
            await organizationRepository.SaveChangesAsync(cancellationToken);

            return organization.Adapt<OrganizationDto>();
        }
    }
}
