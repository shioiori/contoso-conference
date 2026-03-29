using Conference.API.Dtos;
using Conference.API.Services.Abstractions;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Conference.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConferencesController : ControllerBase
    {
        private readonly IConferenceService _conferenceService;

        public ConferencesController(IConferenceService conferenceService)
        {
            _conferenceService = conferenceService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConferenceResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var conferences = await _conferenceService.GetAllAsync(cancellationToken);
            return Ok(conferences.Adapt<IEnumerable<ConferenceResponse>>());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ConferenceResponse>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var conference = await _conferenceService.GetByIdAsync(id, cancellationToken);
            if (conference is null) return NotFound();
            return Ok(conference.Adapt<ConferenceResponse>());
        }

        [HttpPost]
        public async Task<ActionResult<ConferenceResponse>> Create(
            [FromBody] CreateConferenceRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var conference = await _conferenceService.CreateAsync(
                    request.Name,
                    request.Slug,
                    request.StartDate,
                    request.EndDate,
                    request.Description,
                    cancellationToken);

                return CreatedAtAction(nameof(GetById), new { id = conference.Id }, conference.Adapt<ConferenceResponse>());
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ConferenceResponse>> Update(
            Guid id,
            [FromBody] UpdateConferenceRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var conference = await _conferenceService.UpdateAsync(
                    id,
                    request.Name,
                    request.StartDate,
                    request.EndDate,
                    request.Description,
                    cancellationToken);

                return Ok(conference.Adapt<ConferenceResponse>());
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

        [HttpPut("{id:guid}/visibility")]
        public async Task<IActionResult> SetVisibility(
            Guid id,
            [FromBody] SetVisibilityRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                await _conferenceService.SetVisibilityAsync(id, request.IsPublished, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{id:guid}/report")]
        public async Task<ActionResult<ConferenceReportResponse>> GetReport(Guid id, CancellationToken cancellationToken)
        {
            var conference = await _conferenceService.GetByIdAsync(id, cancellationToken);
            if (conference is null) return NotFound();
            return Ok(conference.Adapt<ConferenceReportResponse>());
        }
    }
}
