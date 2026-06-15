using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Eventbox.EventManagement.EventApi.Dtos.PublicEvents;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Mapster;

namespace Eventbox.EventManagement.EventApi.Services
{
    public class PublicEventService(IUnitOfWork unitOfWork) : IPublicEventService
    {
        public async Task<PublicEventDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var Event = await unitOfWork.Events.GetBySlugAsync(slug, cancellationToken);
            return Event?.Adapt<PublicEventDto>();
        }

        public async Task<IEnumerable<PublicEventDto>> SearchAsync(PublicEventSearchDto searchDto, CancellationToken cancellationToken = default)
        {
            var dateFrom = searchDto.DateFrom ?? searchDto.StartDate;
            var dateTo = searchDto.DateTo ?? searchDto.EndDate;

            var page = searchDto.Page <= 0 ? 1 : searchDto.Page;
            var pageSize = searchDto.PageSize <= 0 ? 20 : Math.Min(searchDto.PageSize, 100);

            var Events = await unitOfWork.Events.SearchPublishedAsync(
                searchDto.Status,
                searchDto.Q,
                dateFrom,
                dateTo,
                page,
                pageSize,
                cancellationToken);

            return Events.Adapt<IEnumerable<PublicEventDto>>();
        }
    }
}
