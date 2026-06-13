using Eventbox.EventManagement.EventApi.Domains;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.EventManagement.EventApi.Infrastructure
{
    public class EventDbContext : DbContext
    {
        public EventDbContext(DbContextOptions<EventDbContext> options) : base(options)
        {
        }

        public DbSet<Domains.Event> Events => Set<Domains.Event>();
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<Domains.SeatType> SeatTypes => Set<Domains.SeatType>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventDbContext).Assembly);
        }
    }
}
