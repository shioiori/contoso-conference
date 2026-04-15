using Microsoft.EntityFrameworkCore;
using Registration.Domain.Entities.OrderAggregate;
using Registration.Domain.Entities.SeatAvailabilityAggregate;

namespace Registration.Infrastructure
{
    public class RegistrationDbContext : DbContext
    {
        public RegistrationDbContext(DbContextOptions<RegistrationDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<SeatAvailability> SeatAvailabilities => Set<SeatAvailability>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RegistrationDbContext).Assembly);
        }
    }
}
