using Conference.API.Dtos;
using Conference.API.Services.Abstractions;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Conference.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeatTypesController : ControllerBase
    {
        private readonly ISeatTypeService _seatTypeService;

        public SeatTypesController(ISeatTypeService seatTypeService)
        {
            _seatTypeService = seatTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SeatTypeResponse>>> GetByConference(
            [FromQuery] Guid conferenceId,
            CancellationToken cancellationToken)
        {
            var seatTypes = await _seatTypeService.GetByConferenceIdAsync(conferenceId, cancellationToken);
            return Ok(seatTypes.Adapt<IEnumerable<SeatTypeResponse>>());
        }

        [HttpPost]
        public async Task<ActionResult<SeatTypeResponse>> Create(
            [FromBody] CreateSeatTypeRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var seatType = await _seatTypeService.CreateAsync(request.Name, request.ConferenceId, request.Quota, cancellationToken);
                return CreatedAtAction(nameof(GetByConference), new { conferenceId = seatType.ConferenceId }, seatType.Adapt<SeatTypeResponse>());
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id:int}/seats")]
        public async Task<ActionResult<SeatTypeResponse>> AddSeats(
            int id,
            [FromBody] AddSeatsRequest request,
            CancellationToken cancellationToken)
        {
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
    }
}
