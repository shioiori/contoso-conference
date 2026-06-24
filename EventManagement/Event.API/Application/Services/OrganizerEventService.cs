using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Eventbox.Contracts.IntegrationEvents;
using Eventbox.Shared.Exceptions;
using Mapster;
using System.Security.Cryptography;
using Eventbox.Shared.Outbox;
using System.Text.Json;
using Eventbox.EventManagement.EventApi.Api.Requests;
using Eventbox.EventManagement.EventApi.Application.Dtos.OrganizerEvents;
using Eventbox.EventManagement.EventApi.Application.Abstractions.Services;
using Eventbox.EventManagement.EventApi.Domains;

namespace Eventbox.EventManagement.EventApi.Application.Services
{
    public class OrganizerEventService(IUnitOfWork unitOfWork) : IOrganizerEventService
    {
        public async Task<OrganizerEventDto?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
            => (await unitOfWork.Events.GetByOrganizationAsync(organizationId, id, includeTicketTypes: true, cancellationToken: cancellationToken))?.Adapt<OrganizerEventDto>();

        public async Task<IEnumerable<OrganizerEventDto>> SearchAsync(OrganizerEventSearchRequest searchDto, CancellationToken cancellationToken = default)
        {
            if (!searchDto.OrganizationId.HasValue)
                return [];

            var page = searchDto.Page <= 0 ? 1 : searchDto.Page;
            var pageSize = searchDto.PageSize <= 0 ? 20 : Math.Min(searchDto.PageSize, 100);

            var events = await unitOfWork.Events.SearchForOrganizerAsync(
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

        public async Task<OrganizerEventDto> CreateAsync(Guid organizationId, CreateEventDto dto, CancellationToken cancellationToken = default)
        {
            if (await unitOfWork.Events.SlugExistsAsync(dto.Slug, cancellationToken))
                throw new ConflictException($"An event with slug '{dto.Slug}' already exists.");

            var accessCode = GenerateAccessCode();
            var eventEntity = new Event(Guid.NewGuid(), organizationId, dto.Name, dto.Slug, dto.From, dto.To, dto.Description, accessCode);
            await unitOfWork.Events.AddAsync(eventEntity, cancellationToken);
            var eventCreatedEvent = new EventCreatedEvent
            {
                EventId = eventEntity.Id,
                Name = eventEntity.Name,
                Slug = eventEntity.Slug,
                Description = eventEntity.Description,
                From = eventEntity.From,
                To = eventEntity.To,
                IsPublished = eventEntity.IsPublished,
                AccessCode = accessCode
            };
            await unitOfWork.Outbox.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                IntegrationEventType = nameof(EventCreatedEvent),
                Content = JsonSerializer.Serialize(eventCreatedEvent),
                OccurredOnUtc = DateTime.UtcNow,
                Status = ProcessStatus.Pending
            }, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return eventEntity.Adapt<OrganizerEventDto>();
        }

        public async Task<OrganizerEventDto> UpdateAsync(Guid organizationId, Guid id, UpdateEventDto dto, CancellationToken cancellationToken = default)
        {
            var eventEntity = await unitOfWork.Events.GetByOrganizationAsync(organizationId, id, asNoTracking: false, cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Event", id);

            eventEntity.Update(dto.Name, dto.From, dto.To, dto.Description);

            var eventUpdatedEvent = new EventUpdatedEvent
            {
                EventId = eventEntity.Id,
                Name = eventEntity.Name,
                Description = eventEntity.Description,
                From = eventEntity.From,
                To = eventEntity.To,
                IsPublished = eventEntity.IsPublished,
            };
            await unitOfWork.Outbox.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                IntegrationEventType = nameof(EventUpdatedEvent),
                Content = JsonSerializer.Serialize(eventUpdatedEvent),
                OccurredOnUtc = DateTime.UtcNow,
                Status = ProcessStatus.Pending
            }, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return eventEntity.Adapt<OrganizerEventDto>();
        }

        public async Task DeleteAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
        {
            var Event = await unitOfWork.Events.GetByOrganizationAsync(organizationId, id, asNoTracking: false, cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Event", id);

            if (Event.IsPublished)
                throw new ConflictException("Cannot delete a published event. Unpublish it first.");

            unitOfWork.Events.Delete(Event);
            await unitOfWork.SaveChangesAsync(cancellationToken);
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
            var Event = await unitOfWork.Events.GetByOrganizationAsync(organizationId, id, includeTicketTypes: true, asNoTracking: true, cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Event", id);

            return Event.IsReadyToPublish ? Event.Adapt<OrganizerEventDto>() : null;
        }

        public async Task PublishedAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
        {
            var Event = await unitOfWork.Events.GetByOrganizationAsync(organizationId, id, includeTicketTypes: true, asNoTracking: false, cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Event", id);

            var wasPublished = Event.IsPublished;
            Event.Publish();
            if (wasPublished) return;

            var eventPublishedEvent = new EventPublishedEvent
            {
                EventId = Event.Id,
            };
            await unitOfWork.Outbox.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                IntegrationEventType = nameof(EventPublishedEvent),
                Content = JsonSerializer.Serialize(eventPublishedEvent),
                OccurredOnUtc = DateTime.UtcNow,
                Status = ProcessStatus.Pending
            }, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UnpublishedAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
        {
            var Event = await unitOfWork.Events.GetByOrganizationAsync(organizationId, id, asNoTracking: false, cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Event", id);

            var wasPublished = Event.IsPublished;
            Event.Unpublish();
            if (!wasPublished) return;

            var eventUnpublishedEvent = new EventUnpublishedEvent
            {
                EventId = Event.Id,
            };
            await unitOfWork.Outbox.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                IntegrationEventType = nameof(EventUnpublishedEvent),
                Content = JsonSerializer.Serialize(eventUnpublishedEvent),
                OccurredOnUtc = DateTime.UtcNow,
                Status = ProcessStatus.Pending
            }, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
