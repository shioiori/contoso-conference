using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Shared.Exceptions;
using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Eventbox.Shared.Outbox;
using System.Text.Json;

namespace Eventbox.EventManagement.EventApi.Services
{
    public class TicketTypeService(IUnitOfWork unitOfWork) : ITicketTypeService
    {
        public async Task<TicketType?> GetByIdAsync(Guid organizationId, Guid eventId, Guid id, CancellationToken cancellationToken = default)
        {
            await EnsureEventBelongsToOrganizationAsync(organizationId, eventId, cancellationToken);
            var ticketType = await unitOfWork.TicketTypes.GetByIdAsync(id, cancellationToken);
            return ticketType is not null && ticketType.EventId == eventId ? ticketType : null;
        }

        public async Task<IEnumerable<TicketType>> GetByEventIdAsync(Guid organizationId, Guid eventId, CancellationToken cancellationToken = default)
        {
            await EnsureEventBelongsToOrganizationAsync(organizationId, eventId, cancellationToken);
            return await unitOfWork.TicketTypes.GetByEventIdAsync(eventId, cancellationToken);
        }

        public async Task<TicketType> CreateAsync(Guid organizationId, TicketType ticketType, CancellationToken cancellationToken = default)
        {
            var eventEntity = await EnsureEventBelongsToOrganizationAsync(organizationId, ticketType.EventId, cancellationToken);
            ticketType.ValidatePricingPhasesAgainstEvent(eventEntity.From, eventEntity.To);

            await unitOfWork.TicketTypes.AddAsync(ticketType, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var ticketTypeCreatedEvent = new TicketTypeCreatedEvent
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
                PricingPhases = ticketType.PricingPhases.Select(phase => new TicketTypePricingPhaseSnapshot
                {
                    Id = phase.Id,
                    Name = phase.Name,
                    Price = phase.Price,
                    StartTime = phase.StartTime,
                    EndTime = phase.EndTime,
                }).ToList(),
            };
            await unitOfWork.Outbox.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                IntegrationEventType = nameof(TicketTypeCreatedEvent),
                Content = JsonSerializer.Serialize(ticketTypeCreatedEvent),
                OccurredOnUtc = DateTime.UtcNow,
                Status = ProcessStatus.Pending
            }, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ticketType;
        }

        public async Task<TicketType> UpdateAsync(Guid organizationId, Guid eventId, Guid ticketTypeId, TicketType updated, CancellationToken cancellationToken = default)
        {
            var eventEntity = await EnsureEventBelongsToOrganizationAsync(organizationId, eventId, cancellationToken);

            if (DateTimeOffset.UtcNow >= eventEntity.From)
                throw new ConflictException("Cannot edit ticket types after the event has started.");

            var ticketType = await unitOfWork.TicketTypes.GetByIdAsync(ticketTypeId, cancellationToken);
            if (ticketType is null || ticketType.EventId != eventId)
                throw new NotFoundException("TicketType", ticketTypeId);

            ticketType.Update(
                updated.Name,
                updated.Description,
                updated.Quota,
                updated.Currency,
                updated.MinPerOrder,
                updated.MaxPerOrder,
                updated.Visibility,
                updated.AccessCodeHash,
                updated.PricingPhases);

            ticketType.ValidatePricingPhasesAgainstEvent(eventEntity.From, eventEntity.To);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ticketType;
        }

        public async Task<TicketType> AddCapacityAsync(Guid organizationId, Guid eventId, Guid ticketTypeId, int quantity, CancellationToken cancellationToken = default)
        {
            await EnsureEventBelongsToOrganizationAsync(organizationId, eventId, cancellationToken);

            var ticketType = await unitOfWork.TicketTypes.GetByIdAsync(ticketTypeId, cancellationToken)
                ?? throw new NotFoundException("TicketType", ticketTypeId);

            if (ticketType.EventId != eventId)
                throw new NotFoundException("TicketType", ticketTypeId);

            var previousQuota = ticketType.Quota;
            ticketType.IncreaseQuota(quantity);
            var ticketCapacityAddedEvent = new TicketCapacityAddedEvent
            {
                Id = ticketType.Id,
                EventId = ticketType.EventId,
                PreviousQuantity = previousQuota,
                NewQuantity = ticketType.Quota,
                AddedQuantity = quantity,
            };
            await unitOfWork.Outbox.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                IntegrationEventType = nameof(TicketCapacityAddedEvent),
                Content = JsonSerializer.Serialize(ticketCapacityAddedEvent),
                OccurredOnUtc = DateTime.UtcNow,
                Status = ProcessStatus.Pending
            }, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ticketType;
        }

        private async Task<Event> EnsureEventBelongsToOrganizationAsync(Guid organizationId, Guid eventId, CancellationToken cancellationToken)
        {
            return await unitOfWork.Events.GetByOrganizationAsync(
                    organizationId,
                    eventId,
                    includeTicketTypes: false,
                    asNoTracking: true,
                    cancellationToken)
                ?? throw new NotFoundException($"Event '{eventId}' was not found in organization '{organizationId}'.");
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var ticketType = await unitOfWork.TicketTypes.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("TicketType", id);
            unitOfWork.TicketTypes.Delete(ticketType);
            var ticketTypeDeletedEvent = new TicketTypeDeletedEvent
            {
                Id = ticketType.Id,
                EventId = ticketType.EventId,
            };
            await unitOfWork.Outbox.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                IntegrationEventType = nameof(TicketTypeDeletedEvent),
                Content = JsonSerializer.Serialize(ticketTypeDeletedEvent),
                OccurredOnUtc = DateTime.UtcNow,
                Status = ProcessStatus.Pending
            }, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
