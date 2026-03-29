using Conference.API.Domains;
using Conference.API.IntegrationEvents;
using Conference.API.Repositories.Abstractions;
using Conference.API.Services.Abstractions;
using Contoso.ServiceBus.Abstractions;

namespace Conference.API.Services
{
    public class SeatTypeService : ISeatTypeService
    {
        private readonly ISeatTypeRepository _repository;
        private readonly IConferenceRepository _conferenceRepository;
        private readonly IEventBus _eventBus;

        public SeatTypeService(ISeatTypeRepository repository, IConferenceRepository conferenceRepository, IEventBus eventBus)
        {
            _repository = repository;
            _conferenceRepository = conferenceRepository;
            _eventBus = eventBus;
        }

        public Task<SeatType?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _repository.GetByIdAsync(id, cancellationToken);

        public Task<IEnumerable<SeatType>> GetByConferenceIdAsync(Guid conferenceId, CancellationToken cancellationToken = default)
            => _repository.GetByConferenceIdAsync(conferenceId, cancellationToken);

        public async Task<SeatType> CreateAsync(string name, Guid conferenceId, int quota, CancellationToken cancellationToken = default)
        {
            var conference = await _conferenceRepository.GetByIdAsync(conferenceId, cancellationToken)
                ?? throw new KeyNotFoundException($"Conference '{conferenceId}' not found.");

            var seatType = new SeatType(name, conference.Id, quota);
            await _repository.AddAsync(seatType, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            await _eventBus.PublishAsync(new SeatCreatedEvent
            {
                SeatTypeId = seatType.Id,
                ConferenceId = seatType.ConferenceId,
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
                ConferenceId = seatType.ConferenceId,
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
