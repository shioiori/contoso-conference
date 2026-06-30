using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Domains.Enums;
using Eventbox.Shared.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Event = Eventbox.EventManagement.EventApi.Domains.Event;

namespace Eventbox.EventManagement.EventApi.Infrastructure.Repositories
{
    public class EventRepository(EventDbContext dbContext, TimeProvider timeProvider)
        : BaseRepository<EventDbContext, Event, Guid>(dbContext), IEventRepository
    {
        public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
            => await dbContext.Events.AnyAsync(c => c.Slug == slug, cancellationToken);

        public async Task<Event?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
            => await dbContext.Events
                .AsNoTracking()
                .Include(c => c.TicketTypes)
                    .ThenInclude(t => t.PricingPhases)
                .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);

        public async Task<Event?> GetByOrganizationAsync(Guid organizationId, Guid eventId, bool includeTicketTypes = false, bool asNoTracking = true, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Events.Where(c => c.OrganizationId == organizationId && c.Id == eventId);

            if (includeTicketTypes)
                query = query.Include(c => c.TicketTypes)
                    .ThenInclude(t => t.PricingPhases);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Event>> SearchForOrganizerAsync(Guid organizationId, EventStatus? status, string? query, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var events = dbContext.Events
                .AsNoTracking()
                .Include(c => c.TicketTypes)
                    .ThenInclude(t => t.PricingPhases)
                .Where(c => c.OrganizationId == organizationId);

            events = ApplyFilters(events, status, query, dateFrom, dateTo, timeProvider.GetUtcNow());

            return await events
                .OrderByDescending(c => c.From)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Event>> SearchPublishedAsync(EventStatus? status, string? query, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var events = dbContext.Events
                .AsNoTracking()
                .Include(c => c.TicketTypes)
                    .ThenInclude(t => t.PricingPhases)
                .Where(c => c.IsPublished);

            events = ApplyFilters(events, status, query, dateFrom, dateTo, timeProvider.GetUtcNow());

            return await events
                .OrderBy(c => c.From)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        private static IQueryable<Event> ApplyFilters(IQueryable<Event> events, EventStatus? status, string? query, DateOnly? dateFrom, DateOnly? dateTo, DateTimeOffset utcNow)
        {
            if (status.HasValue)
            {
                var now = utcNow;
                events = status.Value switch
                {
                    EventStatus.Published => events.Where(c => c.IsPublished),
                    EventStatus.Draft => events.Where(c => !c.IsPublished && c.From > now),
                    EventStatus.Cancelled => events.Where(c => !c.IsPublished && c.From <= now),
                    _ => events
                };
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                var searchTerm = query.Trim().ToLower();
                events = events.Where(c =>
                    c.Name.ToLower().Contains(searchTerm) ||
                    c.Slug.ToLower().Contains(searchTerm) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
            }

            if (dateFrom.HasValue)
            {
                var from = new DateTimeOffset(dateFrom.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
                events = events.Where(c => c.From >= from);
            }

            if (dateTo.HasValue)
            {
                var to = new DateTimeOffset(dateTo.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
                events = events.Where(c => c.From <= to);
            }

            return events;
        }
    }
}
