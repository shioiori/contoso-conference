using Microsoft.EntityFrameworkCore;
using Eventbox.Ticketing.Domain.Orders;
using Eventbox.Ticketing.Domain.Enums;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Shared.Infrastructure.Repositories;

namespace Eventbox.Ticketing.Infrastructure.Repositories;

public class OrderRepository(TicketingDbContext dbContext) : BaseRepository<TicketingDbContext, Order, Guid>(dbContext), IOrderRepository
{
    public async Task<IEnumerable<Order>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        => await DbContext.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.EventId == eventId)
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
}
