using Eventbox.EventManagement.EventApi.Api.Requests;
using Eventbox.EventManagement.EventApi.Application.Abstractions.Services;
using Eventbox.EventManagement.EventApi.Application.Dtos;
using Eventbox.Shared.Constants;
using Eventbox.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventbox.EventManagement.EventApi.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = PolicyName.RequireOrganizerAccount)]
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
            [FromBody] OrganizationRequest request,
            CancellationToken cancellationToken)
        {
            var organization = await organizationService.CreateAsync(request.Name, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = organization.Id }, organization);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrganizationDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var organization = await organizationService.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Organization", id);
            return Ok(organization);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<OrganizationDto>> Update(
            Guid id,
            [FromBody] OrganizationRequest request,
            CancellationToken cancellationToken)
        {
            var organization = await organizationService.UpdateAsync(id, request.Name, cancellationToken);
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
            Guid organizerId,
            CancellationToken cancellationToken)
        {
            await organizationService.AddOrganizerAsync(id, organizerId, cancellationToken);
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
