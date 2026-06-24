using Eventbox.Ticketing.Domain.Orders;
using Eventbox.Ticketing.Domain.Enums;
using Eventbox.Shared.Repositories;

namespace Eventbox.Ticketing.Application.Abstractions.Repositories;

public interface IOrderRepository : IRepository<Order, Guid>
{
    Task<IEnumerable<Order>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByStateAsync(OrderState state, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithDetailsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Order?> GetByAccessCodeAsync(string accessCode, CancellationToken cancellationToken = default);
    Task<bool> HasActivePendingOrderAsync(Guid eventId, Guid? userId, string email, DateTimeOffset utcNow, CancellationToken cancellationToken = default);
}
