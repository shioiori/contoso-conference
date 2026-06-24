using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Eventbox.EventManagement.EventApi.Application.Abstractions.Services;
using Eventbox.EventManagement.EventApi.Application.Dtos;
using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.Shared.Exceptions;
using Mapster;
using System.Security.Claims;

namespace Eventbox.EventManagement.EventApi.Application.Services
{
    public class OrganizationService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor) : IOrganizationService
    {
        public async Task<OrganizationDto> CreateAsync(string name, CancellationToken cancellationToken = default)
        {
            var organization = new Organization(Guid.NewGuid(), name);
            await unitOfWork.Organizations.AddAsync(organization, cancellationToken);

            var userIdStr = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId))
                await unitOfWork.OrganizationMembers.AddAsync(new OrganizationMember(organization.Id, userId), cancellationToken);

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

        public async Task<IReadOnlyList<OrganizationDto>> GetMyOrganizationsAsync(CancellationToken cancellationToken = default)
        {
            var userIdStr = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return [];

            var organizations = await unitOfWork.OrganizationMembers.GetOrganizationsByOrganizerAsync(userId, cancellationToken);
            return organizations.Select(o => o.Adapt<OrganizationDto>()).ToList();
        }

        public async Task AddOrganizerAsync(Guid organizationId, Guid organizerId, CancellationToken cancellationToken = default)
        {
            var organization = await unitOfWork.Organizations.GetByIdAsync(organizationId, cancellationToken)
                ?? throw new NotFoundException("Organization", organizationId);

            var alreadyMember = await unitOfWork.OrganizationMembers.ExistsAsync(organizationId, organizerId, cancellationToken);
            if (alreadyMember)
                throw new ConflictException("Organizer is already a member of this organization.");

            var member = new OrganizationMember(organizationId, organizerId);
            await unitOfWork.OrganizationMembers.AddAsync(member, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public Task<bool> IsMemberAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default)
            => unitOfWork.OrganizationMembers.ExistsAsync(organizationId, userId, cancellationToken);

        public async Task RemoveOrganizerAsync(Guid organizationId, Guid organizerId, CancellationToken cancellationToken = default)
        {
            var member = await unitOfWork.OrganizationMembers.GetAsync(organizationId, organizerId, cancellationToken)
                ?? throw new NotFoundException("OrganizationMember", $"{organizationId}/{organizerId}");

            unitOfWork.OrganizationMembers.Delete(member);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<OrganizationDto> UpdateAsync(Guid id, string name, CancellationToken cancellationToken = default)
        {
            var organization = await unitOfWork.Organizations.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Organization", id);

            organization.Update(name);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return organization.Adapt<OrganizationDto>();
        }
    }
}
