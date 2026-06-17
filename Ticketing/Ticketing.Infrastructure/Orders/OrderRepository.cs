using Microsoft.EntityFrameworkCore;
using Eventbox.Ticketing.Domain.Entities.OrderAggregate;
using Eventbox.Ticketing.Domain.Enums;
using Eventbox.Ticketing.Infrastructure.Repositories.Common;
using Eventbox.Ticketing.Application.Abstractions.Repositories;

namespace Eventbox.Ticketing.Infrastructure.Repositories
{
    public class OrderRepository : BaseRepository<TicketingDbContext, Order, Guid>, IOrderRepository
    {
        public OrderRepository(TicketingDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Order>> GetByEventIdAsync(Guid EventId, CancellationToken cancellationToken = default)
            => await DbContext.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.EventId == EventId)
                .ToListAsync(cancellationToken);

        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => await DbContext.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .ToListAsync(cancellationToken);

        public async Task<IEnumerable<Order>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => await DbContext.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.PersonalInfo.Email == email)
                .ToListAsync(cancellationToken);

        public async Task<IEnumerable<Order>> GetByStateAsync(OrderState state, CancellationToken cancellationToken = default)
            => await DbContext.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderState == state)
                .ToListAsync(cancellationToken);

        public async Task<Order?> GetByIdWithDetailsAsync(Guid orderId, CancellationToken cancellationToken = default)
            => await DbContext.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Tickets)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        public async Task<Order?> GetByAccessCodeAsync(string accessCode, CancellationToken cancellationToken = default)
            => await DbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.AccessCode == accessCode, cancellationToken);

        public async Task<bool> HasActivePendingOrderAsync(Guid eventId, Guid? userId, string email, DateTimeOffset utcNow, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = email.Trim().ToUpperInvariant();

            return await DbContext.Orders.AnyAsync(
                o => o.EventId == eventId
                    && o.OrderState == OrderState.Pending
                    && o.ReservationExpiresAt > utcNow
                    && ((userId.HasValue && o.UserId == userId.Value)
                        || o.PersonalInfo.Email.ToUpper() == normalizedEmail),
                cancellationToken);
        }

        public Task<Ticket?> GetTicketByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default)
            => DbContext.Tickets.FirstOrDefaultAsync(t => t.QrTokenHash == qrTokenHash, cancellationToken);

        public Task<bool> ExistsTicketByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default)
            => DbContext.Tickets.AnyAsync(t => t.QrTokenHash == qrTokenHash, cancellationToken);

        public Task<int> TryMarkTicketCheckedInAsync(Guid ticketId, Guid? staffUserId, DateTimeOffset checkedInAt, CancellationToken cancellationToken = default)
            => DbContext.Tickets
                .Where(t => t.Id == ticketId
                    && t.TicketState == TicketState.Active
                    && t.CheckedInAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.CheckedInAt, checkedInAt)
                    .SetProperty(t => t.CheckedInByUserId, staffUserId),
                    cancellationToken);
    }
}
