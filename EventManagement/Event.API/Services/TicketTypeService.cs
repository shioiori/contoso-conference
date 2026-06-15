using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.EventManagement.EventApi.IntegrationEvents;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Shared.Exceptions;
using Eventbox.EventManagement.EventApi.Application.Abstractions;

namespace Eventbox.EventManagement.EventApi.Services
{
    public class TicketTypeService(IUnitOfWork unitOfWork, IEventBus eventBus) : ITicketTypeService
    {
        public async Task<TicketType?> GetByIdAsync(Guid organizationId, Guid eventId, int id, CancellationToken cancellationToken = default)
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
            await EnsureEventBelongsToOrganizationAsync(organizationId, ticketType.EventId, cancellationToken);

            await unitOfWork.TicketTypes.AddAsync(ticketType, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await eventBus.PublishAsync(new TicketTypeCreatedEvent
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
            }, cancellationToken);

            return ticketType;
        }

        public async Task<TicketType> AddCapacityAsync(Guid organizationId, Guid eventId, int ticketTypeId, int quantity, CancellationToken cancellationToken = default)
        {
            await EnsureEventBelongsToOrganizationAsync(organizationId, eventId, cancellationToken);

            var ticketType = await unitOfWork.TicketTypes.GetByIdAsync(ticketTypeId, cancellationToken)
                ?? throw new NotFoundException("TicketType", ticketTypeId);

            if (ticketType.EventId != eventId)
                throw new NotFoundException("TicketType", ticketTypeId);

            var previousQuota = ticketType.Quota;
            ticketType.IncreaseQuota(quantity);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await eventBus.PublishAsync(new TicketCapacityAddedEvent
            {
                Id = ticketType.Id,
                EventId = ticketType.EventId,
                PreviousQuantity = previousQuota,
                NewQuantity = ticketType.Quota,
                AddedQuantity = quantity,
            }, cancellationToken);

            return ticketType;
        }

        private async Task EnsureEventBelongsToOrganizationAsync(Guid organizationId, Guid eventId, CancellationToken cancellationToken)
        {
            _ = await unitOfWork.Events.GetByOrganizationAsync(
                    organizationId,
                    eventId,
                    includeTicketTypes: false,
                    asNoTracking: true,
                    cancellationToken)
                ?? throw new NotFoundException($"Event '{eventId}' was not found in organization '{organizationId}'.");
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var ticketType = await unitOfWork.TicketTypes.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("TicketType", id);
            unitOfWork.TicketTypes.Delete(ticketType);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
