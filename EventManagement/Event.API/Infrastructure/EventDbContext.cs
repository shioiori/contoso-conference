using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.Shared.Outbox;
using Microsoft.EntityFrameworkCore;
using Event = Eventbox.EventManagement.EventApi.Domains.Event;
namespace Eventbox.EventManagement.EventApi.Infrastructure
{
    public class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
    {
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
        public DbSet<TicketType> TicketTypes => Set<TicketType>();
        public DbSet<OutboxMessage> Outboxes => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventDbContext).Assembly);
        }
    }
}
