using Eventbox.EventManagement.EventApi.Api.Requests;
using Eventbox.EventManagement.EventApi.Application.Dtos.PublicEvents;

namespace Eventbox.EventManagement.EventApi.Application.Abstractions.Services
{
    public interface IPublicEventService
    {
        Task<PublicEventDto> GetBySlugAsync(string slug, string? accessCode = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<PublicEventDto>> SearchAsync(PublicEventSearchRequest searchDto, CancellationToken cancellationToken = default);
    }
}
