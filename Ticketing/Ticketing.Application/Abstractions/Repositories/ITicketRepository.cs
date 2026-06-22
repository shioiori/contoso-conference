using Eventbox.Shared.Abstractions;
using Eventbox.Ticketing.Domain.Tickets;

namespace Eventbox.Ticketing.Application.Abstractions.Repositories;

public interface ITicketRepository : IRepository<Ticket, Guid>
{
    Task<IReadOnlyList<Ticket>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetByOrderIdsAsync(IEnumerable<Guid> orderIds, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default);
    Task<bool> ExistsByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default);
    Task<int> TryMarkCheckedInAsync(Guid ticketId, Guid? staffUserId, DateTimeOffset checkedInAt, CancellationToken cancellationToken = default);
}
