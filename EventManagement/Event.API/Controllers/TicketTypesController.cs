using Eventbox.EventManagement.EventApi.Dtos;
using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.EventManagement.EventApi.Requests;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Eventbox.Shared.Exceptions;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventbox.EventManagement.EventApi.Controllers
{
    [ApiController]
    [Authorize(Policy = "RequireOrganizerAccount")]
    [Route("api/organizations/{organizationId:guid}/events/{eventId:guid}/ticket-types")]
    public class TicketTypesController : ControllerBase
    {
        private readonly ITicketTypeService _ticketTypeService;

        public TicketTypesController(ITicketTypeService ticketTypeService)
        {
            _ticketTypeService = ticketTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketTypeDto>>> GetByEvent(
            Guid organizationId,
            Guid eventId,
            CancellationToken cancellationToken)
        {
            var ticketTypes = await _ticketTypeService.GetByEventIdAsync(organizationId, eventId, cancellationToken);
            return Ok(ticketTypes.Adapt<IEnumerable<TicketTypeDto>>());
        }

        [HttpPost]
        public async Task<ActionResult<TicketTypeDto>> Create(
            Guid organizationId,
            Guid eventId,
            [FromBody] CreateTicketTypeRequest request,
            CancellationToken cancellationToken)
        {
            var ticketType = new TicketType(
                request.Name,
                eventId,
                request.Quota,
                request.Description,
                request.Currency,
                request.MinPerOrder,
                request.MaxPerOrder,
                request.Visibility,
                request.AccessCodeHash,
                request.PricingPhases.Select(phase => new PricingPhase(
                    phase.Name,
                    phase.Price,
                    phase.StartTime,
                    phase.EndTime)));

            ticketType = await _ticketTypeService.CreateAsync(organizationId, ticketType, cancellationToken);
            return CreatedAtAction(nameof(GetByEvent), new { organizationId, eventId = ticketType.EventId }, ticketType.Adapt<TicketTypeDto>());
        }

        [HttpPatch("{ticketTypeId}")]
        public async Task<ActionResult<TicketTypeDto>> Update(
            Guid organizationId,
            Guid eventId,
            string ticketTypeId,
            CancellationToken cancellationToken)
        {
            if (!int.TryParse(ticketTypeId, out var id))
                throw new NotFoundException("TicketType", ticketTypeId);

            var ticketType = await _ticketTypeService.GetByIdAsync(organizationId, eventId, id, cancellationToken);
            if (ticketType is null)
                throw new NotFoundException("TicketType", id);

            return Ok(ticketType.Adapt<TicketTypeDto>());
        }

        [HttpPost("{ticketTypeId}/capacity")]
        public async Task<ActionResult<TicketTypeDto>> AddCapacity(
            Guid organizationId,
            Guid eventId,
            string ticketTypeId,
            [FromBody] AddCapacityRequest request,
            CancellationToken cancellationToken)
        {
            if (!int.TryParse(ticketTypeId, out var id))
                throw new NotFoundException("TicketType", ticketTypeId);

            var ticketType = await _ticketTypeService.AddCapacityAsync(organizationId, eventId, id, request.Quantity, cancellationToken);
            return Ok(ticketType.Adapt<TicketTypeDto>());
        }

        [HttpGet("{ticketTypeId}/availability")]
        public async Task<ActionResult<TicketTypeDto>> GetAvailability(
            Guid organizationId,
            Guid eventId,
            string ticketTypeId,
            CancellationToken cancellationToken)
        {
            if (!int.TryParse(ticketTypeId, out var id))
                throw new NotFoundException("TicketType", ticketTypeId);

            var ticketType = await _ticketTypeService.GetByIdAsync(organizationId, eventId, id, cancellationToken);
            if (ticketType is null)
                throw new NotFoundException("TicketType", id);

            return Ok(ticketType.Adapt<TicketTypeDto>());
        }
    }
}
