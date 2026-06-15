using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Dtos.OrganizerEvents;
using Eventbox.EventManagement.EventApi.IntegrationEvents;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Shared.Exceptions;
using Mapster;
using System.Security.Cryptography;

namespace Eventbox.EventManagement.EventApi.Services
{
    public class OrganizerEventService : IOrganizerEventService
    {
        private readonly IEventRepository _repository;
        private readonly IEventBus _eventBus;

        public OrganizerEventService(IEventRepository repository, IEventBus eventBus)
        {
            _repository = repository;
            _eventBus = eventBus;
        }

        public async Task<OrganizerEventDto?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
            => (await _repository.GetByOrganizationAsync(organizationId, id, includeTicketTypes: true, cancellationToken: cancellationToken))?.Adapt<OrganizerEventDto>();

        public async Task<IEnumerable<OrganizerEventDto>> SearchAsync(OrganizerEventSearchDto searchDto, CancellationToken cancellationToken = default)
        {
            if (!searchDto.OrganizationId.HasValue)
                return [];

            var page = searchDto.Page <= 0 ? 1 : searchDto.Page;
            var pageSize = searchDto.PageSize <= 0 ? 20 : Math.Min(searchDto.PageSize, 100);

            var events = await _repository.SearchForOrganizerAsync(
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

        public async Task<OrganizerEventDto> CreateAsync(Guid? organizationId, string name, string slug, DateTimeOffset from, DateTimeOffset to, string? description = null, CancellationToken cancellationToken = default)
        {
            if (await _repository.SlugExistsAsync(slug, cancellationToken))
                throw new ConflictException($"An event with slug '{slug}' already exists.");

            var accessCode = GenerateAccessCode();
            var eventEntity = new Domains.Event(Guid.NewGuid(), organizationId ?? Guid.Empty, name, slug, from, to, description, accessCode);
            await _repository.AddAsync(eventEntity, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            await _eventBus.PublishAsync(new EventCreatedEvent
            {
                EventId = eventEntity.Id,
                Name = eventEntity.Name,
                Slug = eventEntity.Slug,
                Description = eventEntity.Description,
                From = eventEntity.From,
                To = eventEntity.To,
                AccessCode = eventEntity.AccessCode!,
            }, cancellationToken);

            return eventEntity.Adapt<OrganizerEventDto>();
        }

        public async Task<OrganizerEventDto> UpdateAsync(Guid organizationId, Guid id, string name, DateTimeOffset from, DateTimeOffset to, string? description = null, CancellationToken cancellationToken = default)
        {
            var eventEntity = await _repository.GetByOrganizationAsync(organizationId, id, asNoTracking: false, cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Event", id);

            eventEntity.Update(name, from, to, description);
            await _repository.SaveChangesAsync(cancellationToken);

            await _eventBus.PublishAsync(new EventUpdatedEvent
            {
                EventId = eventEntity.Id,
                Name = eventEntity.Name,
                Description = eventEntity.Description,
                From = eventEntity.From,
                To = eventEntity.To,
            }, cancellationToken);

            return eventEntity.Adapt<OrganizerEventDto>();
        }

        public async Task DeleteAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
        {
            var Event = await _repository.GetByOrganizationAsync(organizationId, id, asNoTracking: false, cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Event", id);

            if (Event.IsPublished)
                throw new ConflictException("Cannot delete a published event. Unpublish it first.");

            _repository.Delete(Event);
            await _repository.SaveChangesAsync(cancellationToken);
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
            var Event = await _repository.GetByOrganizationAsync(organizationId, id, includeTicketTypes: true, asNoTracking: true, cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Event", id);

            if (!string.IsNullOrWhiteSpace(Event.Name) &&
                !string.IsNullOrWhiteSpace(Event.Slug) &&
                Event.To > Event.From &&
                Event.TicketTypes.Any())
            {
                return Event.Adapt<OrganizerEventDto>();
            }

            return null;
        }

        public async Task PublishedAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
        {
            var Event = await _repository.GetByOrganizationAsync(organizationId, id, includeTicketTypes: true, asNoTracking: false, cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Event", id);

            if (!string.IsNullOrWhiteSpace(Event.Name) &&
                !string.IsNullOrWhiteSpace(Event.Slug) &&
                Event.To > Event.From &&
                Event.TicketTypes.Any())
            {
                Event.Publish();
            }
            else
            {
                throw new ValidationApiException("Event is not ready to publish.");
            }

            await _repository.SaveChangesAsync(cancellationToken);
            await _eventBus.PublishAsync(new EventPublishedEvent
            {
                EventId = Event.Id,
                Name = Event.Name,
                Slug = Event.Slug,
            }, cancellationToken);
        }

        public async Task UnpublishedAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
        {
            var Event = await _repository.GetByOrganizationAsync(organizationId, id, asNoTracking: false, cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Event", id);

            Event.Unpublish();
            await _repository.SaveChangesAsync(cancellationToken);
            await _eventBus.PublishAsync(new EventUnpublishedEvent
            {
                EventId = Event.Id,
                Name = Event.Name,
                Slug = Event.Slug,
            }, cancellationToken);
        }
    }
}
