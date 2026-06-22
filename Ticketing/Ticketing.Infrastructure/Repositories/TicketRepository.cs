using Eventbox.Shared.Infrastructure.Repositories;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Domain.Enums;
using Eventbox.Ticketing.Domain.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.Ticketing.Infrastructure.Repositories;

public class TicketRepository(TicketingDbContext dbContext) 
    : BaseRepository<TicketingDbContext, Ticket, Guid>(dbContext), ITicketRepository
{
    public async Task<IReadOnlyList<Ticket>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        => await DbContext.Tickets.Where(t => t.OrderId == orderId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Ticket>> GetByOrderIdsAsync(IEnumerable<Guid> orderIds, CancellationToken cancellationToken = default)
        => await DbContext.Tickets.Where(t => orderIds.Contains(t.OrderId)).ToListAsync(cancellationToken);

    public Task<Ticket?> GetByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default)
        => DbContext.Tickets.FirstOrDefaultAsync(t => t.QrTokenHash == qrTokenHash, cancellationToken);

    public Task<bool> ExistsByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default)
        => DbContext.Tickets.AnyAsync(t => t.QrTokenHash == qrTokenHash, cancellationToken);

    public Task<int> TryMarkCheckedInAsync(Guid ticketId, Guid? staffUserId, DateTimeOffset checkedInAt, CancellationToken cancellationToken = default)
        => DbContext.Tickets
            .Where(t => t.Id == ticketId && t.TicketState == TicketState.Active && t.CheckedInAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.CheckedInAt, checkedInAt)
                .SetProperty(t => t.CheckedInByUserId, staffUserId),
                cancellationToken);
}
