using Microsoft.EntityFrameworkCore;
using Registration.Domain.Entities.SeatAvailabilityAggregate;
using Registration.Domain.Repositories;

namespace Registration.Infrastructure.Repositories
{
    public class SeatAvailabilityRepository : ISeatAvailabilityRepository
    {
        private readonly RegistrationDbContext _context;

        public SeatAvailabilityRepository(RegistrationDbContext context)
        {
            _context = context;
        }

        public async Task<SeatAvailability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.SeatAvailabilities
                .Include(s => s.SeatTypes)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        public async Task<IEnumerable<SeatAvailability>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.SeatAvailabilities
                .Include(s => s.SeatTypes)
                .ToListAsync(cancellationToken);

        public async Task<SeatAvailability?> GetByConferenceIdAsync(Guid conferenceId, CancellationToken cancellationToken = default)
            => await _context.SeatAvailabilities
                .Include(s => s.SeatTypes)
                .FirstOrDefaultAsync(s => s.Id == conferenceId, cancellationToken);

        public async Task AddAsync(SeatAvailability entity, CancellationToken cancellationToken = default)
            => await _context.SeatAvailabilities.AddAsync(entity, cancellationToken);

        public void Update(SeatAvailability entity)
            => _context.SeatAvailabilities.Update(entity);

        public void Delete(SeatAvailability entity)
            => _context.SeatAvailabilities.Remove(entity);

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
