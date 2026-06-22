using Eventbox.EventManagement.EventApi.Api.Requests;
using Eventbox.EventManagement.EventApi.Application.Abstractions.Services;
using Eventbox.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Eventbox.EventManagement.EventApi.Api.Controllers
{
    [ApiController]
    [Route("api/public/events")]
    public class PublicEventsController(IPublicEventService publicEventService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PublicEventSearchRequest searchDto, CancellationToken cancellationToken)
        {
            var Events = await publicEventService.SearchAsync(searchDto, cancellationToken);
            return Ok(Events);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetDetail(string slug, [FromQuery] string? accessCode, CancellationToken cancellationToken)
        {
            var Event = await publicEventService.GetBySlugAsync(slug, accessCode, cancellationToken);
            if (Event is null || !Event.IsPublished)
                throw new NotFoundException("Event", slug);

            return Ok(Event);
        }
    }
}
