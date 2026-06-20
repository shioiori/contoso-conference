using Eventbox.EventManagement.EventApi.Dtos;
using Eventbox.EventManagement.EventApi.Requests;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Eventbox.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventbox.EventManagement.EventApi.Controllers
{
    [ApiController]
    [Authorize(Policy = "RequireOrganizerAccount")]
    [Route("api/organizations")]
    public class OrganizationController(IOrganizationService organizationService) : ControllerBase
    {
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetMyOrganizations(CancellationToken cancellationToken)
        {
            var organizations = await organizationService.GetMyOrganizationsAsync(cancellationToken);
            return Ok(organizations);
        }

        [HttpPost]
        public async Task<ActionResult<OrganizationDto>> Create(
            [FromBody] OrganizationDto organizationDto,
            CancellationToken cancellationToken)
        {
            var organization = await organizationService.Create(organizationDto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = organization.Id }, organization);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrganizationDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var organization = await organizationService.GetByIdAsync(id, cancellationToken);
            if (organization is null)
                throw new NotFoundException("Organization", id);

            return Ok(organization);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<OrganizationDto>> Update(
            Guid id,
            [FromBody] OrganizationDto organizationDto,
            CancellationToken cancellationToken)
        {
            var organization = await organizationService.Update(id, organizationDto, cancellationToken);
            return Ok(organization);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await organizationService.Delete(id, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:guid}/organizers")]
        public async Task<IActionResult> AddOrganizer(
            Guid id,
            [FromBody] AddOrganizerRequest request,
            CancellationToken cancellationToken)
        {
            await organizationService.AddOrganizerAsync(id, request.OrganizerId, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}/organizers/{organizerId:guid}")]
        public async Task<IActionResult> RemoveOrganizer(
            Guid id,
            Guid organizerId,
            CancellationToken cancellationToken)
        {
            await organizationService.RemoveOrganizerAsync(id, organizerId, cancellationToken);
            return NoContent();
        }
    }
}
