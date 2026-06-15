using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Eventbox.EventManagement.EventApi.Dtos.OrganizerEvents;
using Eventbox.EventManagement.EventApi.IntegrationEvents;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Eventbox.EventBus.Core.Abstractions;
using Mapster;
using System.Security.Cryptography;

namespace Eventbox.EventManagement.EventApi.Services
{
    public class OrganizerEventService : IOrganizerEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;

        public OrganizerEventService(IUnitOfWork unitOfWork, IEventBus eventBus)
        {
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
        }

        public async Task<OrganizerEventDto?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
            => (await _unitOfWork.Events.GetByOrganizationAsync(organizationId, id, includeSeats: true, cancellationToken: cancellationToken))?.Adapt<OrganizerEventDto>();

        public async Task<IEnumerable<OrganizerEventDto>> SearchAsync(OrganizerEventSearchDto searchDto, CancellationToken cancellationToken = default)
        {
            if (!searchDto.OrganizationId.HasValue)
                return [];

            var page = searchDto.Page <= 0 ? 1 : searchDto.Page;
            var pageSize = searchDto.PageSize <= 0 ? 20 : Math.Min(searchDto.PageSize, 100);

            var events = await _unitOfWork.Events.SearchForOrganizerAsync(
                searchDto.OrganizationId.Value,
                searchDto.Status,
                searchDto.Q,
                searchDto.DateFrom,
                searchDto.DateTo,
                page,
                pageSize,
                cancellationToken);

            return events.Adapt<IEnumerable<OrganizerEventDto>>();
        }

        public async Task<OrganizerEventDto> CreateAsync(Guid? organizationId, string name, string slug, DateOnly startDate, DateOnly endDate, string? description = null, CancellationToken cancellationToken = default)
        {
            if (await _unitOfWork.Events.SlugExistsAsync(slug, cancellationToken))
                throw new InvalidOperationException($"A Event with slug '{slug}' already exists.");

            var accessCode = GenerateAccessCode();
            var eventEntity = new Domains.Event(Guid.NewGuid(), organizationId ?? Guid.Empty, name, slug, startDate, endDate, description, accessCode);
            await _unitOfWork.Events.AddAsync(eventEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventBus.PublishAsync(new EventCreatedEvent
            {
                EventId = eventEntity.Id,
                Name = eventEntity.Name,
                Slug = eventEntity.Slug,
                Description = eventEntity.Description,
                StartDate = eventEntity.StartDate,
                EndDate = eventEntity.EndDate,
                AccessCode = eventEntity.AccessCode!,
            }, cancellationToken);

            return eventEntity.Adapt<OrganizerEventDto>();
        }

        public async Task<OrganizerEventDto> UpdateAsync(Guid organizationId, Guid id, string name, DateOnly startDate, DateOnly endDate, string? description = null, CancellationToken cancellationToken = default)
        {
            var eventEntity = await _unitOfWork.Events.GetByOrganizationAsync(organizationId, id, asNoTracking: false, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Event '{id}' not found.");

            eventEntity.Update(name, startDate, endDate, description);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventBus.PublishAsync(new EventUpdatedEvent
            {
                EventId = eventEntity.Id,
                Name = eventEntity.Name,
                Description = eventEntity.Description,
                StartDate = eventEntity.StartDate,
                EndDate = eventEntity.EndDate,
            }, cancellationToken);

            return eventEntity.Adapt<OrganizerEventDto>();
        }

        public async Task DeleteAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
        {
            var Event = await _unitOfWork.Events.GetByOrganizationAsync(organizationId, id, asNoTracking: false, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Event '{id}' not found.");

            if (Event.IsPublished)
                throw new InvalidOperationException($"Cannot delete a published Event. Unpublish it first.");

            _unitOfWork.Events.Delete(Event);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static string GenerateAccessCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Range(0, 5)
                .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
                .ToArray());
        }

        public async Task<OrganizerEventDto?> GetPublicReadiness(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
        {
            var Event = await _unitOfWork.Events.GetByOrganizationAsync(organizationId, id, includeSeats: true, asNoTracking: true, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Event '{id}' not found.");

            if (!string.IsNullOrWhiteSpace(Event.Name) &&
                !string.IsNullOrWhiteSpace(Event.Slug) &&
                Event.EndDate > Event.StartDate &&
                Event.Seats.Any())
            {
                return Event.Adapt<OrganizerEventDto>();
            }

            return null;
        }

        public async Task PublishedAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
        {
            var Event = await _unitOfWork.Events.GetByOrganizationAsync(organizationId, id, includeSeats: true, asNoTracking: false, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Event '{id}' not found.");

            if (!string.IsNullOrWhiteSpace(Event.Name) &&
                !string.IsNullOrWhiteSpace(Event.Slug) &&
                Event.EndDate > Event.StartDate &&
                Event.Seats.Any())
            {
                Event.Publish();
            }
            else
            {
                throw new InvalidOperationException("Event is not ready to publish.");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _eventBus.PublishAsync(new EventPublishedEvent
            {
                EventId = Event.Id,
                Name = Event.Name,
                Slug = Event.Slug,
            }, cancellationToken);
        }

        public async Task UnpublishedAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
        {
            var Event = await _unitOfWork.Events.GetByOrganizationAsync(organizationId, id, asNoTracking: false, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Event '{id}' not found.");

            Event.Unpublish();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _eventBus.PublishAsync(new EventUnpublishedEvent
            {
                EventId = Event.Id,
                Name = Event.Name,
                Slug = Event.Slug,
            }, cancellationToken);
        }
    }
}
