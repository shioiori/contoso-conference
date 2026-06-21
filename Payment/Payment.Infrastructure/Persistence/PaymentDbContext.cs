using Eventbox.Shared.Outbox;
using Microsoft.EntityFrameworkCore;
using PaymentEntity = Eventbox.Payment.Core.Entities.Payment;

namespace Eventbox.Payment.Infrastructure.Persistence
{
    public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
    {
        public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();
        public DbSet<OutboxMessage> Outboxes => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PaymentEntity>(payment =>
            {
                payment.HasKey(p => p.Id);
                payment.Property(p => p.Amount).HasPrecision(18, 2).IsRequired();
                payment.Property(p => p.Currency).HasMaxLength(3).IsRequired();
                payment.Property(p => p.IdempotencyKey).HasMaxLength(200);
                payment.Property(p => p.ProviderEventId).HasMaxLength(200);
                payment.Property(p => p.FailureReason).HasMaxLength(1000);
                payment.Property(p => p.ReturnUrl).HasMaxLength(2048);
                payment.Property(p => p.CancelUrl).HasMaxLength(2048);
                payment.HasIndex(p => p.IdempotencyKey).IsUnique();
                payment.HasIndex(p => p.ProviderEventId).IsUnique();
                payment.HasIndex(p => p.OrderId)
                    .IsUnique()
                    .HasFilter("\"Status\" = 0")
                    .HasDatabaseName("IX_Payments_OrderId_Pending");
            });
        }
    }
}
