using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.Contracts.IntegrationEvents;
using Eventbox.Shared.Exceptions;
using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Eventbox.EventManagement.EventApi.Application.Dtos;
using Eventbox.Shared.Outbox;
using Mapster;
using System.Text.Json;
using Eventbox.EventManagement.EventApi.Application.Abstractions.Services;
using Eventbox.EventManagement.EventApi.Application.Extensions;

namespace Eventbox.EventManagement.EventApi.Application.Services
{
    public class TicketTypeService(IUnitOfWork unitOfWork) : ITicketTypeService
    {
        public async Task<TicketTypeDto?> GetByIdAsync(Guid organizationId, Guid eventId, Guid id, CancellationToken cancellationToken = default)
        {
            await EnsureEventBelongsToOrganizationAsync(organizationId, eventId, cancellationToken);
            var ticketType = await unitOfWork.TicketTypes.GetByIdAsync(id, cancellationToken);
            return ticketType is not null && ticketType.EventId == eventId ? ticketType.Adapt<TicketTypeDto>() : null;
        }

        public async Task<IEnumerable<TicketTypeDto>> GetByEventIdAsync(Guid organizationId, Guid eventId, CancellationToken cancellationToken = default)
        {
            await EnsureEventBelongsToOrganizationAsync(organizationId, eventId, cancellationToken);
            var ticketTypes = await unitOfWork.TicketTypes.GetByEventIdAsync(eventId, cancellationToken);
            return ticketTypes.Adapt<IEnumerable<TicketTypeDto>>();
        }

        public async Task<TicketTypeDto> CreateAsync(Guid organizationId, Guid eventId, TicketTypeInputDto dto, CancellationToken cancellationToken = default)
        {
            var eventEntity = await EnsureEventBelongsToOrganizationAsync(organizationId, eventId, cancellationToken);

            var ticketType = new TicketType(
                dto.Name,
                eventId,
                dto.Quota,
                dto.Description,
                dto.Currency,
                dto.MinPerOrder,
                dto.MaxPerOrder,
                dto.Visibility,
                dto.AccessCode.Hash(),
                dto.PricingPhases.Select(p => new PricingPhase(p.Name, p.Price, p.StartTime, p.EndTime)));

            ticketType.ValidatePricingPhasesAgainstEvent(eventEntity.From, eventEntity.To);
            await unitOfWork.TicketTypes.AddAsync(ticketType, cancellationToken);
            await unitOfWork.Outbox.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                IntegrationEventType = nameof(TicketTypeCreatedIntegrationEvent),
                Content = JsonSerializer.Serialize(new TicketTypeCreatedIntegrationEvent
                {
                    Id = ticketType.Id,
                    EventId = ticketType.EventId,
                    Name = ticketType.Name,
                    Description = ticketType.Description,
                    Quantity = ticketType.Quota,
                    Currency = ticketType.Currency,
                    MinPerOrder = ticketType.MinPerOrder,
                    MaxPerOrder = ticketType.MaxPerOrder,
                    Visibility = ticketType.Visibility.ToString(),
                    AccessCodeHash = ticketType.AccessCodeHash,
                    PricingPhases = ticketType.PricingPhases.Select(p => new TicketTypePricingPhaseSnapshot
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        StartTime = p.StartTime,
                        EndTime = p.EndTime,
                    }).ToList(),
                }),
                OccurredOnUtc = DateTime.UtcNow,
                Status = ProcessStatus.Pending
            }, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ticketType.Adapt<TicketTypeDto>();
        }

        public async Task<TicketTypeDto> UpdateAsync(Guid organizationId, Guid eventId, Guid ticketTypeId, TicketTypeInputDto dto, CancellationToken cancellationToken = default)
        {
            var eventEntity = await EnsureEventBelongsToOrganizationAsync(organizationId, eventId, cancellationToken);

            if (DateTimeOffset.UtcNow >= eventEntity.From)
                throw new ConflictException("Cannot edit ticket types after the event has started.");

            var ticketType = await unitOfWork.TicketTypes.GetByIdAsync(ticketTypeId, cancellationToken);
            if (ticketType is null || ticketType.EventId != eventId)
                throw new NotFoundException("TicketType", ticketTypeId);

            ticketType.Update(
                dto.Name,
                dto.Description,
                dto.Quota,
                dto.Currency,
                dto.MinPerOrder,
                dto.MaxPerOrder,
                dto.Visibility,
                dto.AccessCode.Hash(),
                dto.PricingPhases.Select(p => new PricingPhase(p.Name, p.Price, p.StartTime, p.EndTime)));

            ticketType.ValidatePricingPhasesAgainstEvent(eventEntity.From, eventEntity.To);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ticketType.Adapt<TicketTypeDto>();
        }

        public async Task<TicketTypeDto> AddCapacityAsync(Guid organizationId, Guid eventId, Guid ticketTypeId, int quantity, CancellationToken cancellationToken = default)
        {
            await EnsureEventBelongsToOrganizationAsync(organizationId, eventId, cancellationToken);

            var ticketType = await unitOfWork.TicketTypes.GetByIdAsync(ticketTypeId, cancellationToken)
                ?? throw new NotFoundException("TicketType", ticketTypeId);

            if (ticketType.EventId != eventId)
                throw new NotFoundException("TicketType", ticketTypeId);

            var previousQuota = ticketType.Quota;
            ticketType.IncreaseQuota(quantity);

            await unitOfWork.Outbox.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                IntegrationEventType = nameof(TicketCapacityAddedIntegrationEvent),
                Content = JsonSerializer.Serialize(new TicketCapacityAddedIntegrationEvent
                {
                    Id = ticketType.Id,
                    EventId = ticketType.EventId,
                    NewQuantity = ticketType.Quota
                }),
                OccurredOnUtc = DateTime.UtcNow,
                Status = ProcessStatus.Pending
            }, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ticketType.Adapt<TicketTypeDto>();
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var ticketType = await unitOfWork.TicketTypes.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("TicketType", id);

            unitOfWork.TicketTypes.Delete(ticketType);

            await unitOfWork.Outbox.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                IntegrationEventType = nameof(TicketTypeDeletedIntegrationEvent),
                Content = JsonSerializer.Serialize(new TicketTypeDeletedIntegrationEvent
                {
                    Id = ticketType.Id,
                    EventId = ticketType.EventId,
                }),
                OccurredOnUtc = DateTime.UtcNow,
                Status = ProcessStatus.Pending
            }, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<Event> EnsureEventBelongsToOrganizationAsync(Guid organizationId, Guid eventId, CancellationToken cancellationToken)
        {
            return await unitOfWork.Events.GetByOrganizationAsync(
                    organizationId, eventId, includeTicketTypes: false, asNoTracking: true, cancellationToken)
                ?? throw new NotFoundException($"Event '{eventId}' was not found in organization '{organizationId}'.");
        }
    }
}
