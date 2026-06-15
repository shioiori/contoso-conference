using Microsoft.EntityFrameworkCore;
using Eventbox.TicketingDomain.Entities.CheckInAggregate;
using Eventbox.TicketingDomain.Entities.OrderAggregate;
using Eventbox.TicketingDomain.Entities.TicketAvailabilityAggregate;

namespace Eventbox.TicketingInfrastructure
{
    public class RegistrationDbContext : DbContext
    {
        public RegistrationDbContext(DbContextOptions<RegistrationDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<CheckInPass> CheckInPasses => Set<CheckInPass>();
        public DbSet<CheckInAttempt> CheckInAttempts => Set<CheckInAttempt>();
        public DbSet<TicketAvailability> TicketAvailabilities => Set<TicketAvailability>();

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
            });

            modelBuilder.Entity<CheckInPass>(pass =>
            {
                pass.HasKey(p => p.Id);
                pass.Property(p => p.AttendeeName).HasMaxLength(200).IsRequired();
                pass.Property(p => p.AttendeeEmail).HasMaxLength(320).IsRequired();
                pass.Property(p => p.QrToken).HasMaxLength(128).IsRequired();
                pass.Property(p => p.QrTokenHash).HasMaxLength(128).IsRequired();
                pass.HasIndex(p => p.QrToken).IsUnique();
                pass.HasIndex(p => p.QrTokenHash).IsUnique();
                pass.HasIndex(p => p.RegistrationTicketId).IsUnique();
                pass.HasIndex(p => new { p.EventId, p.Status });
            });

            modelBuilder.Entity<CheckInAttempt>(attempt =>
            {
                attempt.HasKey(a => a.Id);
                attempt.Property(a => a.QrTokenHash).HasMaxLength(128).IsRequired();
                attempt.Property(a => a.FailureReason).HasMaxLength(500);
                attempt.HasIndex(a => new { a.EventId, a.ScannedAt });
                attempt.HasOne<CheckInPass>()
                    .WithMany()
                    .HasForeignKey(a => a.CheckInPassId)
                    .OnDelete(DeleteBehavior.SetNull);
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
