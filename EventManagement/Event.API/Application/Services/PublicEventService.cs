using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Eventbox.EventManagement.EventApi.Api.Requests;
using Eventbox.EventManagement.EventApi.Application.Dtos.PublicEvents;
using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.EventManagement.EventApi.Domains.Enums;
using Mapster;
using System.Security.Cryptography;
using System.Text;
using Eventbox.EventManagement.EventApi.Application.Abstractions.Services;

namespace Eventbox.EventManagement.EventApi.Application.Services
{
    public class PublicEventService(IUnitOfWork unitOfWork) : IPublicEventService
    {
        public async Task<PublicEventDto?> GetBySlugAsync(string slug, string? accessCode = null, CancellationToken cancellationToken = default)
        {
            var Event = await unitOfWork.Events.GetBySlugAsync(slug, cancellationToken);
            if (Event is null)
                return null;

            var accessCodeHash = string.IsNullOrWhiteSpace(accessCode) ? null : HashAccessCode(accessCode);
            var dto = Event.Adapt<PublicEventDto>();
            dto.TicketTypes = Event.TicketTypes
                .Where(ticketType => IsPubliclyVisible(ticketType, accessCodeHash))
                .Adapt<IEnumerable<PublicTicketTypeDto>>();

            return dto;
        }

        public async Task<IEnumerable<PublicEventDto>> SearchAsync(PublicEventSearchRequest searchDto, CancellationToken cancellationToken = default)
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

        private static bool IsPubliclyVisible(TicketType ticketType, string? accessCodeHash)
            => ticketType.Visibility == TicketVisibility.Public
                || (ticketType.Visibility == TicketVisibility.AccessCode
                    && !string.IsNullOrWhiteSpace(accessCodeHash)
                    && string.Equals(ticketType.AccessCodeHash, accessCodeHash, StringComparison.Ordinal));

        private static string HashAccessCode(string accessCode)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(accessCode.Trim()));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
