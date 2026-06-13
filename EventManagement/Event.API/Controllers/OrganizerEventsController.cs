using Eventbox.EventManagement.EventApi.Dtos;
using Eventbox.EventManagement.EventApi.Dtos.OrganizerEvents;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventbox.EventManagement.EventApi.Controllers
{
    [ApiController]
    [Authorize(Policy = "RequireOrganizerAccount")]
    [Route("api/organizations/{organizationId:guid}/events")]
    public class OrganizerEventsController : ControllerBase
    {
        private readonly IOrganizerEventService _eventService;

        public OrganizerEventsController(IOrganizerEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventResponse>>> GetAll(
            Guid organizationId,
            [FromQuery] OrganizerEventSearchDto searchDto,
            CancellationToken cancellationToken)
        {
            searchDto.OrganizationId = organizationId;
            var Events = await _eventService.SearchAsync(searchDto, cancellationToken);
            return Ok(Events.Adapt<IEnumerable<EventResponse>>());
        }

        [HttpPost]
        public async Task<ActionResult<EventResponse>> CreateDraft(
            Guid organizationId,
            [FromBody] CreateEventRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var Event = await _eventService.CreateAsync(
                    organizationId,
                    request.Name,
                    request.Slug,
                    DateOnly.FromDateTime(request.StartDate.UtcDateTime),
                    DateOnly.FromDateTime(request.EndDate.UtcDateTime),
                    request.Description,
                    cancellationToken);

                return CreatedAtAction(nameof(GetById), new { organizationId, id = Event.Id }, Event.Adapt<EventResponse>());
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

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EventResponse>> GetById(Guid organizationId, Guid id, CancellationToken cancellationToken)
        {
            var Event = await _eventService.GetByIdAsync(organizationId, id, cancellationToken);
            if (Event is null) return NotFound();
            return Ok(Event.Adapt<EventResponse>());
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<EventResponse>> Update(
            Guid id,
            Guid organizationId,
            [FromBody] UpdateEventRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var Event = await _eventService.UpdateAsync(
                    organizationId,
                    id,
                    request.Name,
                    DateOnly.FromDateTime(request.StartDate.UtcDateTime),
                    DateOnly.FromDateTime(request.EndDate.UtcDateTime),
                    request.Description,
                    cancellationToken);

                return Ok(Event.Adapt<EventResponse>());
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

        [HttpGet("{id:guid}/publish-readiness")]
        public async Task<ActionResult<EventResponse>> GetPublishReadiness(Guid organizationId, Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var Event = await _eventService.GetPublicReadiness(organizationId, id, cancellationToken);
                if (Event is null) return BadRequest("Event is not ready to publish.");
                return Ok(Event.Adapt<EventResponse>());
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{id:guid}/publish")]
        public async Task<IActionResult> Publish(
            Guid id,
            Guid organizationId,
            [FromBody] SetVisibilityRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                await _eventService.PublishedAsync(organizationId, id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id:guid}/unpublish")]
        public async Task<IActionResult> Unpublish(
            Guid id,
            Guid organizationId,
            [FromBody] SetVisibilityRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                await _eventService.UnpublishedAsync(organizationId, id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
