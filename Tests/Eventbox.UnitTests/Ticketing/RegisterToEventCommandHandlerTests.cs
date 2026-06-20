using System.Linq.Expressions;
using Eventbox.Shared.Exceptions;
using Eventbox.Shared.Outbox;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.Commands;
using Eventbox.Ticketing.Domain.Entities;
using Eventbox.Ticketing.Domain.Entities.OrderAggregate;
using Eventbox.Ticketing.Domain.Entities.TicketAvailabilityAggregate;
using Eventbox.Ticketing.Domain.Enums;

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
            TicketTypeId = 1,
            Quantity = 1
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    private sealed class StubUnitOfWork(EventSnapshot snapshot) : IUnitOfWork
    {
        public IOrderRepository Orders => new NullOrderRepository();
        public ITicketAvailabilityRepository TicketAvailabilities => new NullTicketAvailabilityRepository();
        public IEventSnapshotRepository EventSnapshots => new StubEventSnapshotRepository(snapshot);
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
    }

    private sealed class NullOrderRepository : IOrderRepository
    {
        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Order?>(null);
        public Task AddAsync(Order entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
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
        public Task<Ticket?> GetTicketByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default) => Task.FromResult<Ticket?>(null);
        public Task<bool> ExistsTicketByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<int> TryMarkTicketCheckedInAsync(Guid ticketId, Guid? staffUserId, DateTimeOffset checkedInAt, CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private sealed class NullTicketAvailabilityRepository : ITicketAvailabilityRepository
    {
        public Task<TicketAvailability?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default) => Task.FromResult<TicketAvailability?>(null);
        public Task<bool> TryReserveAsync(Guid eventId, int ticketTypeId, int quantity, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<TicketAvailability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<TicketAvailability?>(null);
        public Task AddAsync(TicketAvailability entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(TicketAvailability entity) { }
        public void Delete(TicketAvailability entity) { }
        public IQueryable<TicketAvailability> Get(Expression<Func<TicketAvailability, bool>> filter = null!, Func<IQueryable<TicketAvailability>, IOrderedQueryable<TicketAvailability>> orderBy = null!, string includeProperties = null!, bool needAsNoTracking = true) => Enumerable.Empty<TicketAvailability>().AsQueryable();
    }

    private sealed class NullOutbox : IOutbox
    {
        public Task AddAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken) => Task.CompletedTask;
        public void Update(OutboxMessage outboxMessage) { }
        public Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken) => Task.FromResult(new List<OutboxMessage>());
    }
}
