using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.EventManagement.EventApi.Dtos;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Eventbox.Shared.Exceptions;
using Mapster;

namespace Eventbox.EventManagement.EventApi.Services
{
    public class OrganizationService(IUnitOfWork unitOfWork) : IOrganizationService
    {
        public async Task<OrganizationDto> Create(OrganizationDto organizationDto, CancellationToken cancellationToken = default)
        {
            var id = organizationDto.Id == Guid.Empty ? Guid.NewGuid() : organizationDto.Id;
            var organization = new Organization(id, organizationDto.Name);

            await unitOfWork.Organizations.AddAsync(organization, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return organization.Adapt<OrganizationDto>();
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var organization = await unitOfWork.Organizations.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Organization", id);

            unitOfWork.Organizations.Delete(organization);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return (await unitOfWork.Organizations.GetByIdAsync(id, cancellationToken))?.Adapt<OrganizationDto>();
        }

        public async Task<OrganizationDto> Update(Guid id, OrganizationDto organizationDto, CancellationToken cancellationToken = default)
        {
            var organization = await unitOfWork.Organizations.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Organization", id);

            organization.Update(organizationDto.Name);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return organization.Adapt<OrganizationDto>();
        }
    }
}
