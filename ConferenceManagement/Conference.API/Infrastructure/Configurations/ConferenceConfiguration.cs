using Conference.API.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conference.API.Infrastructure.Configurations
{
    public class ConferenceConfiguration : IEntityTypeConfiguration<Conference.API.Domains.Conference>
    {
        public void Configure(EntityTypeBuilder<Conference.API.Domains.Conference> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

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

            builder.Property(c => c.StartDate)
                .IsRequired();

            builder.Property(c => c.EndDate)
                .IsRequired();

            builder.HasMany<SeatType>(c => c.Seats)
                .WithOne(s => s.Conference)
                .HasForeignKey(s => s.ConferenceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
