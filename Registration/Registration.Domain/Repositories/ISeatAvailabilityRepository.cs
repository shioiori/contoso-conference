using Registration.Domain.Entities.SeatAvailabilityAggregate;

namespace Registration.Domain.Repositories
{
    public interface ISeatAvailabilityRepository : IRepository<SeatAvailability, Guid>
    {
        Task<SeatAvailability?> GetByConferenceIdAsync(Guid conferenceId, CancellationToken cancellationToken = default);
    }
}
