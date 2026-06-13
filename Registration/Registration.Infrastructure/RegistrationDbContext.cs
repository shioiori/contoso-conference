using Microsoft.EntityFrameworkCore;
using Eventbox.Registration.Domain.Entities.OrderAggregate;
using Eventbox.Registration.Domain.Entities.SeatAvailabilityAggregate;

namespace Eventbox.Registration.Infrastructure
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

            modelBuilder.Entity<Order>(order =>
            {
                order.HasKey(o => o.Id);
                order.OwnsOne(o => o.PersonalInfo);
                order.HasMany(o => o.OrderItems)
                    .WithOne()
                    .OnDelete(DeleteBehavior.Cascade);
                order.Navigation(o => o.OrderItems)
                    .UsePropertyAccessMode(PropertyAccessMode.Field);
            });

            modelBuilder.Entity<OrderItem>(item =>
            {
                item.HasKey(i => i.Id);
            });

            modelBuilder.Entity<SeatAvailability>(seatAvailability =>
            {
                seatAvailability.HasKey(s => s.Id);
                seatAvailability.OwnsMany(s => s.TicketTypes, ticketType =>
                {
                    ticketType.WithOwner().HasForeignKey("SeatAvailabilityId");
                    ticketType.Property<int>("Id");
                    ticketType.HasKey("Id");
                });
                seatAvailability.Navigation(s => s.TicketTypes)
                    .UsePropertyAccessMode(PropertyAccessMode.Field);
            });
        }
    }
}
