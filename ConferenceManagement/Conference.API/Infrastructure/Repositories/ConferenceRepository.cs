using Conference.API.Infrastructure;
using Conference.API.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Conference.API.Infrastructure.Repositories
{
    public class ConferenceRepository : IConferenceRepository
    {
        private readonly ConferenceDbContext _context;

        public ConferenceRepository(ConferenceDbContext context)
        {
            _context = context;
        }

        public async Task<Domains.Conference?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Conferences.Include(c => c.Seats).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        public async Task<IEnumerable<Domains.Conference>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.Conferences.Include(c => c.Seats).ToListAsync(cancellationToken);

        public async Task<Domains.Conference?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
            => await _context.Conferences.Include(c => c.Seats).FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);

        public async Task<IEnumerable<Domains.Conference>> GetPublishedAsync(CancellationToken cancellationToken = default)
            => await _context.Conferences.Include(c => c.Seats).Where(c => c.IsPublished).ToListAsync(cancellationToken);

        public async Task AddAsync(Domains.Conference entity, CancellationToken cancellationToken = default)
            => await _context.Conferences.AddAsync(entity, cancellationToken);

        public void Update(Domains.Conference entity)
            => _context.Conferences.Update(entity);

        public void Delete(Domains.Conference entity)
            => _context.Conferences.Remove(entity);

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
