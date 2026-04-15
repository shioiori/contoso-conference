using Conference.API.Domains;
using Conference.API.Infrastructure;
using Conference.API.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Conference.API.Infrastructure.Repositories
{
    public class SeatTypeRepository : ISeatTypeRepository
    {
        private readonly ConferenceDbContext _context;

        public SeatTypeRepository(ConferenceDbContext context)
        {
            _context = context;
        }

        public async Task<SeatType?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _context.SeatTypes.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        public async Task<IEnumerable<SeatType>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.SeatTypes.ToListAsync(cancellationToken);

        public async Task<IEnumerable<SeatType>> GetByConferenceIdAsync(Guid conferenceId, CancellationToken cancellationToken = default)
            => await _context.SeatTypes.Where(s => s.ConferenceId == conferenceId).ToListAsync(cancellationToken);

        public async Task AddAsync(SeatType entity, CancellationToken cancellationToken = default)
            => await _context.SeatTypes.AddAsync(entity, cancellationToken);

        public void Update(SeatType entity)
            => _context.SeatTypes.Update(entity);

        public void Delete(SeatType entity)
            => _context.SeatTypes.Remove(entity);

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
