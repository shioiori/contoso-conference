using Eventbox.EventManagement.EventApi.Api.Requests;
using Eventbox.EventManagement.EventApi.Application.Abstractions.Services;
using Eventbox.EventManagement.EventApi.Application.Dtos;
using Eventbox.Shared.Constants;
using Eventbox.Shared.Exceptions;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventbox.EventManagement.EventApi.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = PolicyName.RequireOrganizerAccount)]
    [Route("api/organizations/{organizationId:guid}/events/{eventId:guid}/ticket-types")]
    public class TicketTypesController(ITicketTypeService ticketTypeService, IOrganizationService organizationService)
        : OrganizerControllerBase(organizationService)
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketTypeDto>>> GetByEvent(
            Guid organizationId,
            Guid eventId,
            CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);
            var ticketTypes = await ticketTypeService.GetByEventIdAsync(organizationId, eventId, cancellationToken);
            return Ok(ticketTypes);
        }

        [HttpPost]
        public async Task<ActionResult<TicketTypeDto>> Create(
            Guid organizationId,
            Guid eventId,
            [FromBody] TicketTypeRequest request,
            CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);
            var ticketType = await ticketTypeService.CreateAsync(organizationId, eventId, request.Adapt<TicketTypeInputDto>(), cancellationToken);
            return CreatedAtAction(nameof(GetByEvent), new { organizationId, eventId }, ticketType);
        }

        [HttpPut("{ticketTypeId:guid}")]
        public async Task<ActionResult<TicketTypeDto>> Update(
            Guid organizationId,
            Guid eventId,
            Guid ticketTypeId,
            [FromBody] TicketTypeRequest request,
            CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);
            var ticketType = await ticketTypeService.UpdateAsync(organizationId, eventId, ticketTypeId, request.Adapt<TicketTypeInputDto>(), cancellationToken);
            return Ok(ticketType);
        }

        [HttpPost("{ticketTypeId:guid}/capacity")]
        public async Task<ActionResult<TicketTypeDto>> AddCapacity(
            Guid organizationId,
            Guid eventId,
            Guid ticketTypeId,
            [FromBody] AddCapacityRequest request,
            CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);
            var ticketType = await ticketTypeService.AddCapacityAsync(organizationId, eventId, ticketTypeId, request.Quantity, cancellationToken);
            return Ok(ticketType);
        }

        [HttpGet("{ticketTypeId:guid}/availability")]
        public async Task<ActionResult<TicketTypeDto>> GetAvailability(
            Guid organizationId,
            Guid eventId,
            Guid ticketTypeId,
            CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);
            var ticketType = await ticketTypeService.GetByIdAsync(organizationId, eventId, ticketTypeId, cancellationToken)
                ?? throw new NotFoundException("TicketType", ticketTypeId);
            return Ok(ticketType);
        }
    }
}
