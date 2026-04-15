using Conference.API.Domains;

namespace Conference.API.Infrastructure.Repositories.Abstractions
{
    public interface ISeatTypeRepository : IRepository<SeatType, int>
    {
        Task<IEnumerable<SeatType>> GetByConferenceIdAsync(Guid conferenceId, CancellationToken cancellationToken = default);
    }
}
