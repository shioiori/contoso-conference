using Eventbox.EventManagement.EventApi.Dtos.PublicEvents;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Eventbox.EventManagement.EventApi.Controllers
{
    [ApiController]
    [Route("api/public/events")]
    public class PublicEventsController(IPublicEventService publicEventService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PublicEventSearchDto searchDto, CancellationToken cancellationToken)
        {
            var Events = await publicEventService.SearchAsync(searchDto, cancellationToken);
            return Ok(Events);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetDetail(string slug, [FromQuery] string? accessCode, CancellationToken cancellationToken)
        {
            var Event = await publicEventService.GetBySlugAsync(slug, cancellationToken);
            if (Event is null || !Event.IsPublished)
                return NotFound();

            return Ok(Event);
        }
    }
}
