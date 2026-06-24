using Eventbox.EventManagement.EventApi.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Event = Eventbox.EventManagement.EventApi.Domains.Event;

namespace Eventbox.EventManagement.EventApi.Infrastructure.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.OrganizationId)
                .IsRequired();

            builder.Property(c => c.Slug)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(c => c.Slug)
                .IsUnique();

            builder.Property(c => c.Description)
                .HasMaxLength(2000);

            builder.Property(c => c.AccessCode)
                .HasMaxLength(10);

            builder.Property(c => c.IsPublished)
                .IsRequired();

            builder.Property(c => c.From)
                .IsRequired();

            builder.Property(c => c.To)
                .IsRequired();

            builder.HasMany<TicketType>(c => c.TicketTypes)
                .WithOne(s => s.Event)
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
