using Microsoft.EntityFrameworkCore;
using Registration.Domain.Entities.OrderAggregate;
using Registration.Domain.Enums;
using Registration.Domain.Repositories;

namespace Registration.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly RegistrationDbContext _context;

        public OrderRepository(RegistrationDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.Orders
                .Include(o => o.OrderItems)
                .ToListAsync(cancellationToken);

        public async Task<IEnumerable<Order>> GetByConferenceIdAsync(Guid conferenceId, CancellationToken cancellationToken = default)
            => await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.ConferenceId == conferenceId)
                .ToListAsync(cancellationToken);

        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .ToListAsync(cancellationToken);

        public async Task<IEnumerable<Order>> GetByStateAsync(OrderState state, CancellationToken cancellationToken = default)
            => await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderState == state)
                .ToListAsync(cancellationToken);

        public async Task<Order?> GetByAccessCodeAsync(string accessCode, CancellationToken cancellationToken = default)
            => await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.AccessCode == accessCode, cancellationToken);

        public async Task AddAsync(Order entity, CancellationToken cancellationToken = default)
            => await _context.Orders.AddAsync(entity, cancellationToken);

        public void Update(Order entity)
            => _context.Orders.Update(entity);

        public void Delete(Order entity)
            => _context.Orders.Remove(entity);

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
