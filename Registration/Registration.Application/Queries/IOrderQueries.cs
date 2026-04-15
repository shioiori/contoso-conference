using Registration.Domain.Entities.OrderAggregate;

namespace Registration.Application.Queries
{
    public interface IOrderQueries
    {
        public Task<IEnumerable<Order>> GetOrdersByEmail(string email);
        public Task<Order> GetOrderDetail(Guid orderId);
    }
}