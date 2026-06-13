using Eventbox.EventManagement.EventApi.Dtos;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventbox.EventManagement.EventApi.Controllers
{
    [ApiController]
    [Authorize(Policy = "RequireOrganizerAccount")]
    [Route("api/organizations/{organizationId:guid}/events/{EventId:guid}/ticket-types")]
    public class SeatTypesController : ControllerBase
    {
        private readonly ISeatTypeService _seatTypeService;

        public SeatTypesController(ISeatTypeService seatTypeService)
        {
            _seatTypeService = seatTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SeatTypeResponse>>> GetByEvent(
            Guid organizationId,
            Guid EventId,
            CancellationToken cancellationToken)
        {
            var seatTypes = await _seatTypeService.GetByEventIdAsync(EventId, cancellationToken);
            return Ok(seatTypes.Adapt<IEnumerable<SeatTypeResponse>>());
        }

        [HttpPost]
        public async Task<ActionResult<SeatTypeResponse>> Create(
            Guid organizationId,
            Guid EventId,
            [FromBody] CreateSeatTypeRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var seatType = await _seatTypeService.CreateAsync(request.Name, EventId, request.Quota, cancellationToken);
                return CreatedAtAction(nameof(GetByEvent), new { organizationId, EventId = seatType.EventId }, seatType.Adapt<SeatTypeResponse>());
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPatch("{ticketTypeId}")]
        public async Task<ActionResult<SeatTypeResponse>> Update(
            Guid organizationId,
            Guid EventId,
            string ticketTypeId,
            CancellationToken cancellationToken)
        {
            if (!int.TryParse(ticketTypeId, out var id))
                return NotFound();

            var seatType = await _seatTypeService.GetByIdAsync(id, cancellationToken);
            if (seatType is null || seatType.EventId != EventId)
                return NotFound();

            return Ok(seatType.Adapt<SeatTypeResponse>());
        }

        [HttpPost("{ticketTypeId}/seats")]
        public async Task<ActionResult<SeatTypeResponse>> AddSeats(
            Guid organizationId,
            Guid EventId,
            string ticketTypeId,
            [FromBody] AddSeatsRequest request,
            CancellationToken cancellationToken)
        {
            if (!int.TryParse(ticketTypeId, out var id))
                return NotFound();

            try
            {
                var seatType = await _seatTypeService.AddSeatsAsync(id, request.Quantity, cancellationToken);
                return Ok(seatType.Adapt<SeatTypeResponse>());
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{ticketTypeId}/availability")]
        public async Task<ActionResult<SeatTypeResponse>> GetAvailability(
            Guid organizationId,
            Guid EventId,
            string ticketTypeId,
            CancellationToken cancellationToken)
        {
            if (!int.TryParse(ticketTypeId, out var id))
                return NotFound();

            var seatType = await _seatTypeService.GetByIdAsync(id, cancellationToken);
            if (seatType is null || seatType.EventId != EventId)
                return NotFound();

            return Ok(seatType.Adapt<SeatTypeResponse>());
        }
    }
}
