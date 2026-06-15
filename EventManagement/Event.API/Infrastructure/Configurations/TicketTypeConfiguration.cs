using Eventbox.EventManagement.EventApi.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventbox.EventManagement.EventApi.Infrastructure.Configurations
{
    public class TicketTypeConfiguration : IEntityTypeConfiguration<TicketType>
    {
        public void Configure(EntityTypeBuilder<TicketType> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .ValueGeneratedOnAdd();

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.Description)
                .HasMaxLength(1000);

            builder.Property(s => s.Quota)
                .IsRequired();

            builder.Property(s => s.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(s => s.MinPerOrder)
                .IsRequired();

            builder.Property(s => s.Visibility)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.AccessCodeHash)
                .HasMaxLength(256);

            builder.Property(s => s.EventId)
                .IsRequired();

            builder.OwnsMany(s => s.PricingPhases, phase =>
            {
                phase.WithOwner().HasForeignKey("TicketTypeId");
                phase.Property(p => p.Id).ValueGeneratedOnAdd();
                phase.HasKey(p => p.Id);
                phase.Property(p => p.Name).IsRequired().HasMaxLength(120);
                phase.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
            });

            builder.Navigation(s => s.PricingPhases)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
