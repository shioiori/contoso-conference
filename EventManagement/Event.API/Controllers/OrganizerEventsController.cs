using Eventbox.EventManagement.EventApi.Dtos;
using Eventbox.EventManagement.EventApi.Dtos.OrganizerEvents;
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
    [Route("api/organizations/{organizationId:guid}/events")]
    public class OrganizerEventsController : ControllerBase
    {
        private readonly IOrganizerEventService _eventService;

        public OrganizerEventsController(IOrganizerEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetAll(
            Guid organizationId,
            [FromQuery] OrganizerEventSearchDto searchDto,
            CancellationToken cancellationToken)
        {
            searchDto.OrganizationId = organizationId;
            var Events = await _eventService.SearchAsync(searchDto, cancellationToken);
            return Ok(Events.Adapt<IEnumerable<EventDto>>());
        }

        [HttpPost]
        public async Task<ActionResult<EventDto>> CreateDraft(
            Guid organizationId,
            [FromBody] CreateEventRequest request,
            CancellationToken cancellationToken)
        {
            var Event = await _eventService.CreateAsync(
                organizationId,
                request.Name,
                request.Slug,
                request.From,
                request.To,
                request.Description,
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { organizationId, id = Event.Id }, Event.Adapt<EventDto>());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EventDto>> GetById(Guid organizationId, Guid id, CancellationToken cancellationToken)
        {
            var Event = await _eventService.GetByIdAsync(organizationId, id, cancellationToken);
            if (Event is null)
                throw new NotFoundException("Event", id);

            return Ok(Event.Adapt<EventDto>());
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<EventDto>> Update(
            Guid id,
            Guid organizationId,
            [FromBody] UpdateEventRequest request,
            CancellationToken cancellationToken)
        {
            var Event = await _eventService.UpdateAsync(
                organizationId,
                id,
                request.Name,
                request.From,
                request.To,
                request.Description,
                cancellationToken);

            return Ok(Event.Adapt<EventDto>());
        }

        [HttpGet("{id:guid}/publish-readiness")]
        public async Task<ActionResult<EventDto>> GetPublishReadiness(Guid organizationId, Guid id, CancellationToken cancellationToken)
        {
            var Event = await _eventService.GetPublicReadiness(organizationId, id, cancellationToken);
            if (Event is null)
                throw new ValidationApiException("Event is not ready to publish.");

            return Ok(Event.Adapt<EventDto>());
        }

        [HttpPost("{id:guid}/publish")]
        public async Task<IActionResult> Publish(
            Guid id,
            Guid organizationId,
            [FromBody] SetVisibilityRequest request,
            CancellationToken cancellationToken)
        {
            await _eventService.PublishedAsync(organizationId, id, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:guid}/unpublish")]
        public async Task<IActionResult> Unpublish(
            Guid id,
            Guid organizationId,
            [FromBody] SetVisibilityRequest request,
            CancellationToken cancellationToken)
        {
            await _eventService.UnpublishedAsync(organizationId, id, cancellationToken);
            return NoContent();
        }
    }
}
