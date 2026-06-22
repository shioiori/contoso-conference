using Eventbox.EventManagement.EventApi.Api.Requests;
using Eventbox.EventManagement.EventApi.Application.Dtos;
using Eventbox.EventManagement.EventApi.Application.Dtos.OrganizerEvents;
using Mapster;
using Eventbox.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Eventbox.EventManagement.EventApi.Application.Abstractions.Services;

namespace Eventbox.EventManagement.EventApi.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = PolicyName.RequireOrganizerAccount)]
    [Route("api/organizations/{organizationId:guid}/events")]
    public class OrganizerEventsController(IOrganizerEventService eventService, IOrganizationService organizationService)
        : OrganizerControllerBase(organizationService)
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetAll(
            Guid organizationId,
            [FromQuery] OrganizerEventSearchRequest searchDto,
            CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);
            searchDto.OrganizationId = organizationId;
            var Events = await eventService.SearchAsync(searchDto, cancellationToken);
            return Ok(Events.Adapt<IEnumerable<EventDto>>());
        }

        [HttpPost]
        public async Task<ActionResult<EventDto>> CreateDraft(
            Guid organizationId,
            [FromBody] CreateEventRequest request,
            CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);
            var Event = await eventService.CreateAsync(organizationId, request.Adapt<CreateEventDto>(), cancellationToken);

            return CreatedAtAction(nameof(GetById), new { organizationId, id = Event.Id }, Event.Adapt<EventDto>());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EventDto>> GetById(Guid organizationId, Guid id, CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);
            var Event = await eventService.GetByIdAsync(organizationId, id, cancellationToken);
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
            await EnsureMemberAsync(organizationId, cancellationToken);
            var Event = await eventService.UpdateAsync(organizationId, id, request.Adapt<UpdateEventDto>(), cancellationToken);

            return Ok(Event.Adapt<EventDto>());
        }

        [HttpGet("{id:guid}/publish-readiness")]
        public async Task<ActionResult<EventDto>> GetPublishReadiness(Guid organizationId, Guid id, CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);
            var Event = await eventService.GetPublicReadiness(organizationId, id, cancellationToken);
            if (Event is null)
                throw new ValidationApiException("Event is not ready to publish.");

            return Ok(Event.Adapt<EventDto>());
        }

        [HttpPost("{id:guid}/publish")]
        public async Task<IActionResult> Publish(
            Guid id,
            Guid organizationId,
            CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);
            await eventService.PublishedAsync(organizationId, id, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:guid}/unpublish")]
        public async Task<IActionResult> Unpublish(
            Guid id,
            Guid organizationId,
            CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);
            await eventService.UnpublishedAsync(organizationId, id, cancellationToken);
            return NoContent();
        }

    }
}
