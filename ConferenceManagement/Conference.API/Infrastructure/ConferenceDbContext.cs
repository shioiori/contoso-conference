using Conference.API.Domains;
using Microsoft.EntityFrameworkCore;

namespace Conference.API.Infrastructure
{
    public class ConferenceDbContext : DbContext
    {
        public ConferenceDbContext(DbContextOptions<ConferenceDbContext> options) : base(options)
        {
        }

        public DbSet<Domains.Conference> Conferences => Set<Domains.Conference>();
        public DbSet<Domains.SeatType> SeatTypes => Set<Domains.SeatType>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConferenceDbContext).Assembly);
        }
    }
}
