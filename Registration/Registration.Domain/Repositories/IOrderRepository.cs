using Registration.Domain.Entities.OrderAggregate;
using Registration.Domain.Enums;

namespace Registration.Domain.Repositories
{
    public interface IOrderRepository : IRepository<Order, Guid>
    {
        Task<IEnumerable<Order>> GetByConferenceIdAsync(Guid conferenceId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> GetByStateAsync(OrderState state, CancellationToken cancellationToken = default);
        Task<Order?> GetByAccessCodeAsync(string accessCode, CancellationToken cancellationToken = default);
    }
}
