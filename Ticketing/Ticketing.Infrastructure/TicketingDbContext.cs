using Microsoft.EntityFrameworkCore;
using Eventbox.Ticketing.Domain.Entities;
using Eventbox.Ticketing.Domain.Entities.OrderAggregate;
using Eventbox.Ticketing.Domain.Entities.TicketAvailabilityAggregate;

namespace Eventbox.Ticketing.Infrastructure
{
    public class TicketingDbContext : DbContext
    {
        public TicketingDbContext(DbContextOptions<TicketingDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<EventSchedule> EventSchedules => Set<EventSchedule>();
        public DbSet<TicketAvailability> TicketAvailabilities => Set<TicketAvailability>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketingDbContext).Assembly);

            modelBuilder.Entity<Order>(order =>
            {
                order.HasKey(o => o.Id);
                order.OwnsOne(o => o.PersonalInfo);
                order.HasMany(o => o.OrderItems)
                    .WithOne()
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
                order.Navigation(o => o.OrderItems)
                    .UsePropertyAccessMode(PropertyAccessMode.Field);
                order.HasMany(o => o.Tickets)
                    .WithOne()
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
                order.Navigation(o => o.Tickets)
                    .UsePropertyAccessMode(PropertyAccessMode.Field);
            });

            modelBuilder.Entity<OrderItem>(item =>
            {
                item.HasKey(i => i.Id);
                item.Property(i => i.TicketTypeName).HasMaxLength(200).IsRequired();
                item.Property(i => i.PricingPhaseName).HasMaxLength(120).IsRequired();
                item.Property(i => i.UnitPrice).HasPrecision(18, 2).IsRequired();
                item.Property(i => i.Currency).HasMaxLength(3).IsRequired();
            });

            modelBuilder.Entity<Ticket>(ticket =>
            {
                ticket.HasKey(t => t.Id);
                ticket.Property(t => t.QrToken).HasMaxLength(128);
                ticket.Property(t => t.QrTokenHash).HasMaxLength(128);
                ticket.HasIndex(t => t.QrToken).IsUnique();
                ticket.HasIndex(t => t.QrTokenHash).IsUnique();
                ticket.HasIndex(t => new { t.EventId, t.TicketState });
            });

            modelBuilder.Entity<EventSchedule>(schedule =>
            {
                schedule.HasKey(e => e.Id);
                schedule.Property(e => e.From).IsRequired();
                schedule.Property(e => e.To).IsRequired();
            });

            modelBuilder.Entity<TicketAvailability>(ticketAvailability =>
            {
                ticketAvailability.HasKey(s => s.Id);
                ticketAvailability.OwnsMany(s => s.TicketTypes, ticketType =>
                {
                    ticketType.WithOwner().HasForeignKey("TicketAvailabilityId");
                    ticketType.HasKey(t => t.Id);
                    ticketType.Property(t => t.Name).HasMaxLength(200).IsRequired();
                    ticketType.Property(t => t.Currency).HasMaxLength(3).IsRequired();
                    ticketType.Property(t => t.Visibility).HasConversion<string>().HasMaxLength(50).IsRequired();
                    ticketType.Property(t => t.AccessCodeHash).HasMaxLength(256);
                    ticketType.OwnsMany(t => t.PricingPhases, phase =>
                    {
                        phase.WithOwner().HasForeignKey("TicketTypeAvailabilityId");
                        phase.HasKey(p => p.Id);
                        phase.Property(p => p.Name).HasMaxLength(120).IsRequired();
                        phase.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
                    });
                    ticketType.Navigation(t => t.PricingPhases)
                        .UsePropertyAccessMode(PropertyAccessMode.Field);
                });
                ticketAvailability.Navigation(s => s.TicketTypes)
                    .UsePropertyAccessMode(PropertyAccessMode.Field);
            });
        }
    }
}
