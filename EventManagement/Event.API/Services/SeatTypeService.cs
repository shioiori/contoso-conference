using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.EventManagement.EventApi.IntegrationEvents;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Eventbox.EventBus.Core.Abstractions;

namespace Eventbox.EventManagement.EventApi.Services
{
    public class SeatTypeService : ISeatTypeService
    {
        private readonly ISeatTypeRepository _repository;
        private readonly IEventRepository _eventRepository;
        private readonly IEventBus _eventBus;

        public SeatTypeService(ISeatTypeRepository repository, IEventRepository EventRepository, IEventBus eventBus)
        {
            _repository = repository;
            _eventRepository = EventRepository;
            _eventBus = eventBus;
        }

        public Task<SeatType?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _repository.GetByIdAsync(id, cancellationToken);

        public Task<IEnumerable<SeatType>> GetByEventIdAsync(Guid EventId, CancellationToken cancellationToken = default)
            => _repository.GetByEventIdAsync(EventId, cancellationToken);

        public async Task<SeatType> CreateAsync(string name, Guid EventId, int quota, CancellationToken cancellationToken = default)
        {
            var Event = await _eventRepository.GetByIdAsync(EventId, cancellationToken)
                ?? throw new KeyNotFoundException($"Event '{EventId}' not found.");

            var seatType = new SeatType(name, Event.Id, quota);
            await _repository.AddAsync(seatType, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            await _eventBus.PublishAsync(new SeatCreatedEvent
            {
                SeatTypeId = seatType.Id,
                EventId = seatType.EventId,
                Name = seatType.Name,
                Quota = seatType.Quota,
            }, cancellationToken);

            return seatType;
        }

        public async Task<SeatType> AddSeatsAsync(int seatTypeId, int quantity, CancellationToken cancellationToken = default)
        {
            var seatType = await _repository.GetByIdAsync(seatTypeId, cancellationToken)
                ?? throw new KeyNotFoundException($"SeatType '{seatTypeId}' not found.");

            var previousQuota = seatType.Quota;
            seatType.IncreaseQuota(quantity);
            await _repository.SaveChangesAsync(cancellationToken);

            await _eventBus.PublishAsync(new SeatsAddedEvent
            {
                SeatTypeId = seatType.Id,
                EventId = seatType.EventId,
                PreviousQuota = previousQuota,
                NewQuota = seatType.Quota,
                AddedQuantity = quantity,
            }, cancellationToken);

            return seatType;
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var seatType = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"SeatType '{id}' not found.");
            _repository.Delete(seatType);
            await _repository.SaveChangesAsync(cancellationToken);
        }
    }
}
