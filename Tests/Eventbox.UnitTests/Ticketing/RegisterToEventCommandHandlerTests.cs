using System.Linq.Expressions;
using Eventbox.Shared.Exceptions;
using Eventbox.Shared.Outbox;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.Commands.Orders;
using Eventbox.Ticketing.Domain.Events;
using Eventbox.Ticketing.Domain.Orders;
using Eventbox.Ticketing.Domain.Inventory;
using Eventbox.Ticketing.Domain.Enums;
using Eventbox.Ticketing.Domain.Tickets;

namespace Eventbox.UnitTests.Ticketing;

public class RegisterToEventCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenEventIsUnpublished_ThrowsNotFoundException()
    {
        var eventId = Guid.NewGuid();
        var unpublishedSnapshot = new EventSnapshot(
            eventId,
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(2),
            isPublished: false);

        var handler = new RegisterToEventCommandHandler(
            new StubUnitOfWork(unpublishedSnapshot));

        var command = new RegisterToEventCommand
        {
            EventId = eventId,
            Name = "Buyer",
            Email = "buyer@example.com",
            TicketTypeId = Guid.NewGuid(),
            Quantity = 1
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    private sealed class StubUnitOfWork(EventSnapshot snapshot) : IUnitOfWork
    {
        public IOrderRepository Orders => new NullOrderRepository();
        public ITicketTypeAvailabilityRepository TicketTypeAvailabilities => new NullTicketTypeAvailabilityRepository();
        public IEventSnapshotRepository EventSnapshots => new StubEventSnapshotRepository(snapshot);
        public ITicketRepository Tickets => new NullTicketRepository();
        public IOutbox Outbox => new NullOutbox();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(1);

        public Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
            => operation();
    }

    private sealed class StubEventSnapshotRepository(EventSnapshot snapshot) : IEventSnapshotRepository
    {
        public Task<EventSnapshot?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
            => Task.FromResult(eventId == snapshot.Id ? snapshot : (EventSnapshot?)null);

        public Task UpsertAsync(Guid eventId, DateTimeOffset? from, DateTimeOffset? to, bool? isPublished, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<EventSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<EventSnapshot?>(null);
        public Task AddAsync(EventSnapshot entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<EventSnapshot> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(EventSnapshot entity) { }
        public void Delete(EventSnapshot entity) { }
        public IQueryable<EventSnapshot> Get(Expression<Func<EventSnapshot, bool>> filter = null!, Func<IQueryable<EventSnapshot>, IOrderedQueryable<EventSnapshot>> orderBy = null!, string includeProperties = null!, bool needAsNoTracking = true) => Enumerable.Empty<EventSnapshot>().AsQueryable();
    }

    private sealed class NullOrderRepository : IOrderRepository
    {
        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Order?>(null);
        public Task AddAsync(Order entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(Order entity) { }
        public void Delete(Order entity) { }
        public IQueryable<Order> Get(Expression<Func<Order, bool>> filter = null!, Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = null!, string includeProperties = null!, bool needAsNoTracking = true) => Enumerable.Empty<Order>().AsQueryable();
        public Task<IEnumerable<Order>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<Order>());
        public Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<Order>());
        public Task<IEnumerable<Order>> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<Order>());
        public Task<IEnumerable<Order>> GetByStateAsync(OrderState state, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<Order>());
        public Task<Order?> GetByIdWithDetailsAsync(Guid orderId, CancellationToken cancellationToken = default) => Task.FromResult<Order?>(null);
        public Task<Order?> GetByAccessCodeAsync(string accessCode, CancellationToken cancellationToken = default) => Task.FromResult<Order?>(null);
        public Task<bool> HasActivePendingOrderAsync(Guid eventId, Guid? userId, string email, DateTimeOffset utcNow, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    private sealed class NullTicketTypeAvailabilityRepository : ITicketTypeAvailabilityRepository
    {
        public Task<TicketTypeAvailability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<TicketTypeAvailability?>(null);
        public Task<TicketTypeAvailability?> GetByIdWithPricingAsync(Guid ticketTypeId, CancellationToken cancellationToken = default) => Task.FromResult<TicketTypeAvailability?>(null);
        public Task<IReadOnlyList<TicketTypeAvailability>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<TicketTypeAvailability>>([]);
        public Task<bool> TryReserveAsync(Guid ticketTypeId, int quantity, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(TicketTypeAvailability entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<TicketTypeAvailability> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(TicketTypeAvailability entity) { }
        public void Delete(TicketTypeAvailability entity) { }
        public IQueryable<TicketTypeAvailability> Get(Expression<Func<TicketTypeAvailability, bool>> filter = null!, Func<IQueryable<TicketTypeAvailability>, IOrderedQueryable<TicketTypeAvailability>> orderBy = null!, string includeProperties = null!, bool needAsNoTracking = true) => Enumerable.Empty<TicketTypeAvailability>().AsQueryable();
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

    private sealed class NullOutbox : IOutbox
    {
        public Task AddAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken) => Task.CompletedTask;
        public void Update(OutboxMessage outboxMessage) { }
        public Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken) => Task.FromResult(new List<OutboxMessage>());
    }
}
