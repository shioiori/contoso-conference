using Conference.API.Domains;

namespace Conference.API.Services.Abstractions
{
    public interface ISeatTypeService
    {
        Task<SeatType?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<SeatType>> GetByConferenceIdAsync(Guid conferenceId, CancellationToken cancellationToken = default);
        Task<SeatType> CreateAsync(string name, Guid conferenceId, int quota, CancellationToken cancellationToken = default);
        Task<SeatType> AddSeatsAsync(int seatTypeId, int quantity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
