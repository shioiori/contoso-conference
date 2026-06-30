using System.Linq.Expressions;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Domain.Orders;
using Eventbox.Ticketing.Domain.Inventory;
using Eventbox.Ticketing.Domain.Enums;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Domain.Events;
using Eventbox.Shared.Exceptions;
using Eventbox.Shared.Outbox;
using Eventbox.Ticketing.Domain.Tickets;
using Eventbox.Ticketing.Application.Commands.Orders;
using Eventbox.Ticketing.Application.Dtos.Orders;

namespace Eventbox.UnitTests.Ticketing;

public class RegisterToEventCommandHandlerConcurrencyTests
{
    [Fact]
    public async Task RegisterToEventAsync_WhenTwoRequestsRaceForLastTicket_DoesNotOversell()
    {
        var eventId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();
        var ticketType = new TicketTypeAvailability(ticketTypeId, eventId, quantity: 1);
        var orderRepository = new InMemoryOrderRepository();
        var ticketTypeAvailabilityRepository = new InMemoryTicketTypeAvailabilityRepository(ticketType);
        var eventSnapshotRepository = new InMemoryEventSnapshotRepository(
            new EventSnapshot(eventId, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1), isPublished: true));
        var unitOfWork = new InMemoryRegistrationUnitOfWork(orderRepository, ticketTypeAvailabilityRepository, eventSnapshotRepository);
        var handler = new RegisterToEventCommandHandler(unitOfWork, TimeProvider.System);

        var firstRequest = CreateRequest(eventId, ticketTypeId, "first@example.com");
        var secondRequest = CreateRequest(eventId, ticketTypeId, "second@example.com");

        var attempts = await Task.WhenAll(
            CaptureResult(() => handler.Handle(firstRequest, CancellationToken.None)),
            CaptureResult(() => handler.Handle(secondRequest, CancellationToken.None)));

        Assert.Single(attempts, attempt => attempt.Order is not null);
        Assert.Single(attempts, attempt => attempt.Exception is ConflictException);
        Assert.Equal(0, ticketType.Remaining);
        Assert.Single(orderRepository.Orders);
        Assert.Single(unitOfWork.OutboxMessages);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    private static RegisterToEventCommand CreateRequest(Guid eventId, Guid ticketTypeId, string email)
        => new()
        {
            EventId = eventId,
            Name = "Buyer",
            Email = email,
            TicketTypeId = ticketTypeId,
            Quantity = 1
        };

    private static async Task<(OrderDto? Order, Exception? Exception)> CaptureResult(Func<Task<OrderDto>> action)
    {
        try
        {
            return (await action(), null);
        }
        catch (Exception ex)
        {
            return (null, ex);
        }
    }

    private sealed class InMemoryTicketTypeAvailabilityRepository(TicketTypeAvailability ticketType) : ITicketTypeAvailabilityRepository
    {
        private readonly object _gate = new();

        public Task<TicketTypeAvailability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == ticketType.Id ? ticketType : null);

        public Task<TicketTypeAvailability?> GetByIdWithPricingAsync(Guid ticketTypeId, CancellationToken cancellationToken = default)
            => Task.FromResult(ticketTypeId == ticketType.Id ? ticketType : null);

        public Task<IReadOnlyList<TicketTypeAvailability>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TicketTypeAvailability>>(
                ticketType.EventId == eventId ? [ticketType] : []);

        public Task<bool> TryReserveAsync(Guid ticketTypeId, int quantity, CancellationToken cancellationToken = default)
        {
            lock (_gate)
            {
                if (ticketTypeId != ticketType.Id)
                    return Task.FromResult(false);

                try
                {
                    ticketType.Reserve(quantity);
                    return Task.FromResult(true);
                }
                catch (InvalidOperationException)
                {
                    return Task.FromResult(false);
                }
            }
        }

        public Task AddAsync(TicketTypeAvailability entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<TicketTypeAvailability> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(TicketTypeAvailability entity) { }
        public void Delete(TicketTypeAvailability entity) { }
        public IQueryable<TicketTypeAvailability> Get(Expression<Func<TicketTypeAvailability, bool>> filter = null!, Func<IQueryable<TicketTypeAvailability>, IOrderedQueryable<TicketTypeAvailability>> orderBy = null!, string includeProperties = null!, bool needAsNoTracking = true) => Enumerable.Empty<TicketTypeAvailability>().AsQueryable();
    }

    private sealed class InMemoryOrderRepository : IOrderRepository
    {
        private readonly List<Order> _orders = new();

        public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_orders.FirstOrDefault(order => order.Id == id));

        public Task AddAsync(Order entity, CancellationToken cancellationToken = default)
        {
            _orders.Add(entity);
            return Task.CompletedTask;
        }

        public Task AddRangeAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default)
        {
            _orders.AddRange(entities);
            return Task.CompletedTask;
        }

        public void Update(Order entity) { }
        public void Delete(Order entity) => _orders.Remove(entity);

        public IQueryable<Order> Get(
            Expression<Func<Order, bool>> filter = null!,
            Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = null!,
            string includeProperties = null!,
            bool needAsNoTracking = true)
        {
            var query = _orders.AsQueryable();
            return filter is null ? query : query.Where(filter);
        }

        public Task<IEnumerable<Order>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
            => Task.FromResult(_orders.Where(order => order.EventId == eventId));

        public Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(_orders.Where(order => order.UserId == userId));

        public Task<IEnumerable<Order>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(_orders.Where(order => order.PersonalInfo.Email == email));

        public Task<IEnumerable<Order>> GetByStateAsync(OrderState state, CancellationToken cancellationToken = default)
            => Task.FromResult(_orders.Where(order => order.OrderState == state));

        public Task<Order?> GetByIdWithDetailsAsync(Guid orderId, CancellationToken cancellationToken = default)
            => GetByIdAsync(orderId, cancellationToken);

        public Task<Order?> GetByAccessCodeAsync(string accessCode, CancellationToken cancellationToken = default)
            => Task.FromResult(_orders.FirstOrDefault(order => order.AccessCode == accessCode));

        public Task<bool> HasActivePendingOrderAsync(
            Guid eventId,
            Guid? userId,
            string email,
            DateTimeOffset utcNow,
            CancellationToken cancellationToken = default)
        {
            var hasOrder = _orders.Any(order =>
                order.EventId == eventId &&
                order.PersonalInfo.Email == email &&
                order.GetCurrentState(utcNow) == OrderState.Pending);

            return Task.FromResult(hasOrder);
        }
    }

    private sealed class InMemoryEventSnapshotRepository(EventSnapshot eventSnapshot) : IEventSnapshotRepository
    {
        public Task<EventSnapshot?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
            => Task.FromResult(eventId == eventSnapshot.Id ? eventSnapshot : null);

        public Task UpsertAsync(Guid eventId, DateTimeOffset? from, DateTimeOffset? to, bool? isPublished, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<EventSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<EventSnapshot?>(null);
        public Task AddAsync(EventSnapshot entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<EventSnapshot> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(EventSnapshot entity) { }
        public void Delete(EventSnapshot entity) { }
        public IQueryable<EventSnapshot> Get(Expression<Func<EventSnapshot, bool>> filter = null!, Func<IQueryable<EventSnapshot>, IOrderedQueryable<EventSnapshot>> orderBy = null!, string includeProperties = null!, bool needAsNoTracking = true) => Enumerable.Empty<EventSnapshot>().AsQueryable();
    }

    private sealed class InMemoryRegistrationUnitOfWork(
        IOrderRepository orderRepository,
        ITicketTypeAvailabilityRepository ticketTypeAvailabilityRepository,
        IEventSnapshotRepository eventSnapshotRepository) : IUnitOfWork
    {
        private readonly InMemoryOutbox _outbox = new();

        public IOrderRepository Orders { get; } = orderRepository;
        public ITicketTypeAvailabilityRepository TicketTypeAvailabilities { get; } = ticketTypeAvailabilityRepository;
        public IEventSnapshotRepository EventSnapshots { get; } = eventSnapshotRepository;
        public ITicketRepository Tickets { get; } = new NullTicketRepository();
        public IOutbox Outbox => _outbox;
        public IReadOnlyCollection<OutboxMessage> OutboxMessages => _outbox.Messages;

        public int SaveCount { get; set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }

        public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
            => await operation();
    }

    private sealed class NullTicketRepository : ITicketRepository
    {
        public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Ticket?>(null);
        public Task AddAsync(Ticket entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<Ticket> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(Ticket entity) { }
        public void Delete(Ticket entity) { }
        public IQueryable<Ticket> Get(Expression<Func<Ticket, bool>> filter = null!, Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>> orderBy = null!, string includeProperties = null!, bool needAsNoTracking = true) => Enumerable.Empty<Ticket>().AsQueryable();
        public Task<IReadOnlyList<Ticket>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Ticket>>([]);
        public Task<IReadOnlyList<Ticket>> GetByOrderIdsAsync(IEnumerable<Guid> orderIds, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Ticket>>([]);
        public Task<Ticket?> GetByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default) => Task.FromResult<Ticket?>(null);
        public Task<bool> ExistsByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<int> TryMarkCheckedInAsync(Guid ticketId, Guid? staffUserId, DateTimeOffset checkedInAt, CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private sealed class InMemoryOutbox : IOutbox
    {
        private readonly List<OutboxMessage> _messages = new();

        public IReadOnlyCollection<OutboxMessage> Messages => _messages.AsReadOnly();

        public Task AddAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
        {
            _messages.Add(outboxMessage);
            return Task.CompletedTask;
        }

        public void Update(OutboxMessage outboxMessage) { }

        public Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken)
            => Task.FromResult(_messages.Take(batchSize).ToList());
    }
}
