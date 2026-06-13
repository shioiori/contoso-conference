using Eventbox.EventManagement.EventApi.Dtos;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventbox.EventManagement.EventApi.Controllers
{
    [ApiController]
    [Authorize(Policy = "RequireOrganizerAccount")]
    [Route("api/organizations")]
    public class OrganizationController(IOrganizationService organizationService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<OrganizationDto>> Create(
            [FromBody] OrganizationDto organizationDto,
            CancellationToken cancellationToken)
        {
            try
            {
                var organization = await organizationService.Create(organizationDto, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = organization.Id }, organization);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrganizationDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var organization = await organizationService.GetByIdAsync(id, cancellationToken);
            if (organization is null) return NotFound();

            return Ok(organization);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<OrganizationDto>> Update(
            Guid id,
            [FromBody] OrganizationDto organizationDto,
            CancellationToken cancellationToken)
        {
            try
            {
                var organization = await organizationService.Update(id, organizationDto, cancellationToken);
                return Ok(organization);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await organizationService.Delete(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
