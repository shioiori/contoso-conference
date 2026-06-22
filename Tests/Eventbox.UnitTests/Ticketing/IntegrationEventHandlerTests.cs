using System.Linq.Expressions;
using Eventbox.Contracts.IntegrationEvents;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.Commands;
using Eventbox.Ticketing.Application.IntegrationEventHandlers;
using Eventbox.Ticketing.Domain.Events;
using Eventbox.Ticketing.Domain.Orders;
using Eventbox.Ticketing.Domain.Inventory;
using Eventbox.Ticketing.Domain.Enums;
using Eventbox.Ticketing.Domain.Tickets;
using Eventbox.Shared.Exceptions;
using Eventbox.Shared.Outbox;
using MediatR;

namespace Eventbox.UnitTests.Ticketing;

// ════════════════════════════════════════════════════════════════════════
// PaymentFailedIntegrationEventHandler
// ════════════════════════════════════════════════════════════════════════

public class PaymentFailedIntegrationEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenOrderExists_MarksPaymentFailedAndSavesOnce()
    {
        var order = PendingOrder();
        var repo = new StubOrderRepository(order);
        var uow = new CountingUnitOfWork();
        var handler = new PaymentFailedIntegrationEventHandler(repo, uow);

        await handler.HandleAsync(new PaymentFailedIntegrationEvent { OrderId = order.Id });

        Assert.Equal(OrderState.PaymentFailed, order.OrderState);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_WhenOrderNotFound_DoesNothing()
    {
        var repo = new StubOrderRepository(null);
        var uow = new CountingUnitOfWork();
        var handler = new PaymentFailedIntegrationEventHandler(repo, uow);

        await handler.HandleAsync(new PaymentFailedIntegrationEvent { OrderId = Guid.NewGuid() });

        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_WhenOrderAlreadyConfirmed_DoesNotSave()
    {
        var order = PendingOrder();
        order.Confirm();
        var repo = new StubOrderRepository(order);
        var uow = new CountingUnitOfWork();
        var handler = new PaymentFailedIntegrationEventHandler(repo, uow);

        await handler.HandleAsync(new PaymentFailedIntegrationEvent { OrderId = order.Id });

        Assert.Equal(OrderState.Confirmed, order.OrderState);
        Assert.Equal(0, uow.SaveCount);
    }

    private static Order PendingOrder()
        => new(Guid.NewGuid(), null,
            new PersonalInfo("Buyer", "buyer@example.com"),
            [new OrderItem(ticketTypeId: Guid.NewGuid(), quantity: 1)],
            accessCode: "TESTCODE",
            DateTimeOffset.UtcNow.AddMinutes(15));
}

// ════════════════════════════════════════════════════════════════════════
// PaymentConfirmedIntegrationEventHandler
// ════════════════════════════════════════════════════════════════════════

public class PaymentConfirmedIntegrationEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_DispatchesConfirmOrderCommandWithCorrectOrderId()
    {
        var mediator = new RecordingMediator();
        var handler = new PaymentConfirmedIntegrationEventHandler(mediator);
        var orderId = Guid.NewGuid();

        await handler.HandleAsync(new PaymentConfirmedIntegrationEvent { OrderId = orderId });

        var cmd = Assert.Single(mediator.SentRequests.OfType<ConfirmOrderCommand>());
        Assert.Equal(orderId, cmd.OrderId);
    }
}

// ════════════════════════════════════════════════════════════════════════
// EventCreatedEventHandler & EventUpdatedEventHandler
// ════════════════════════════════════════════════════════════════════════

public class EventSnapshotHandlerTests
{
    private static readonly Guid EventId = Guid.NewGuid();
    private static readonly DateTimeOffset From = new(2025, 9, 1, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset To = new(2025, 9, 1, 22, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task EventCreated_UpsertsSnapshotAndSaves()
    {
        var repo = new SpyEventSnapshotRepository();
        var uow = new CountingUnitOfWork();
        var handler = new EventCreatedEventHandler(repo, uow);

        await handler.HandleAsync(new EventCreatedEvent { EventId = EventId, From = From, To = To });

        Assert.Single(repo.UpsertCalls, c => c.EventId == EventId && c.From == From && c.To == To && c.IsPublished == false);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task EventUpdated_UpsertsSnapshotWithNewDatesAndSaves()
    {
        var repo = new SpyEventSnapshotRepository();
        var uow = new CountingUnitOfWork();
        var handler = new EventUpdatedEventHandler(repo, uow);
        var newFrom = From.AddDays(7);
        var newTo   = To.AddDays(7);

        await handler.HandleAsync(new EventUpdatedEvent { EventId = EventId, From = newFrom, To = newTo });

        Assert.Single(repo.UpsertCalls, c => c.EventId == EventId && c.From == newFrom && c.To == newTo && c.IsPublished == false);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task EventPublished_UpdatesPublishedStateAndSaves()
    {
        var repo = new SpyEventSnapshotRepository();
        var uow = new CountingUnitOfWork();
        var handler = new EventPublishedEventHandler(repo, uow);

        await handler.HandleAsync(new EventPublishedEvent { EventId = EventId });

        Assert.Single(repo.UpsertCalls, c => c.EventId == EventId && c.From is null && c.To is null && c.IsPublished == true);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task EventUnpublished_UpdatesPublishedStateAndSaves()
    {
        var repo = new SpyEventSnapshotRepository();
        var uow = new CountingUnitOfWork();
        var handler = new EventUnpublishedEventHandler(repo, uow);

        await handler.HandleAsync(new EventUnpublishedEvent { EventId = EventId });

        Assert.Single(repo.UpsertCalls, c => c.EventId == EventId && c.From is null && c.To is null && c.IsPublished == false);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task EventCreated_WhenReceivedTwice_UpsertsSnapshotTwice()
    {
        // Upserting is idempotent by design; handler should call UpsertAsync each time
        var repo = new SpyEventSnapshotRepository();
        var uow = new CountingUnitOfWork();
        var handler = new EventCreatedEventHandler(repo, uow);
        var @event = new EventCreatedEvent { EventId = EventId, From = From, To = To };

        await handler.HandleAsync(@event);
        await handler.HandleAsync(@event);

        Assert.Equal(2, repo.UpsertCalls.Count);
        Assert.Equal(2, uow.SaveCount);
    }
}

// ════════════════════════════════════════════════════════════════════════
// TicketTypeCreatedEventHandler
// ════════════════════════════════════════════════════════════════════════

public class TicketTypeCreatedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenNoAvailabilityExists_CreatesNewAvailabilityWithTicketType()
    {
        var repo = new InMemoryTicketAvailabilityRepository();
        var uow = new CountingUnitOfWork();
        var handler = new TicketTypeCreatedEventHandler(repo, uow);
        var eventId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();

        await handler.HandleAsync(TicketTypeCreatedFor(eventId, ticketTypeId));

        var availability = repo.Stored[eventId];
        Assert.NotNull(availability);
        Assert.Single(availability.TicketTypes, tt => tt.Id == ticketTypeId);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_WhenAvailabilityExistsAndTicketTypeIsNew_AddsTicketType()
    {
        var eventId = Guid.NewGuid();
        var existing = new TicketAvailability(eventId, [new TicketTypeAvailability(Guid.NewGuid(), 50)]);
        var repo = new InMemoryTicketAvailabilityRepository(existing);
        var uow = new CountingUnitOfWork();
        var handler = new TicketTypeCreatedEventHandler(repo, uow);

        await handler.HandleAsync(TicketTypeCreatedFor(eventId, Guid.NewGuid()));

        Assert.Equal(2, existing.TicketTypes.Count);
    }

    [Fact]
    public async Task HandleAsync_WhenTicketTypeAlreadyExists_DoesNotAddDuplicate()
    {
        var eventId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();
        var existing = new TicketAvailability(eventId, [new TicketTypeAvailability(ticketTypeId, 50)]);
        var repo = new InMemoryTicketAvailabilityRepository(existing);
        var uow = new CountingUnitOfWork();
        var handler = new TicketTypeCreatedEventHandler(repo, uow);

        await handler.HandleAsync(TicketTypeCreatedFor(eventId, ticketTypeId));

        Assert.Single(existing.TicketTypes);
    }

    [Fact]
    public async Task HandleAsync_WhenVisibilityIsInvalidString_DefaultsToPublic()
    {
        var repo = new InMemoryTicketAvailabilityRepository();
        var uow = new CountingUnitOfWork();
        var handler = new TicketTypeCreatedEventHandler(repo, uow);
        var eventId = Guid.NewGuid();
        var @event = TicketTypeCreatedFor(eventId, Guid.NewGuid());
        @event.Visibility = "nonexistent_visibility";

        await handler.HandleAsync(@event);

        var tt = repo.Stored[eventId].TicketTypes.Single();
        Assert.Equal(TicketVisibility.Public, tt.Visibility);
    }

    private static TicketTypeCreatedEvent TicketTypeCreatedFor(Guid eventId, Guid ticketTypeId) => new()
    {
        Id = ticketTypeId,
        EventId = eventId,
        Name = "General",
        Quantity = 100,
        Currency = "VND",
        MinPerOrder = 1,
        Visibility = "Public",
        PricingPhases = [new TicketTypePricingPhaseSnapshot { Id = Guid.NewGuid(), Name = "Standard", Price = 0 }]
    };
}

// ════════════════════════════════════════════════════════════════════════
// TicketCapacityAddedEventHandler
// ════════════════════════════════════════════════════════════════════════

public class TicketCapacityAddedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_IncreasesTicketTypeQuantityAndSaves()
    {
        var eventId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();
        var ticketType = new TicketTypeAvailability(ticketTypeId, quantity: 50);
        var availability = new TicketAvailability(eventId, [ticketType]);
        var repo = new InMemoryTicketAvailabilityRepository(availability);
        var uow = new CountingUnitOfWork();
        var handler = new TicketCapacityAddedEventHandler(repo, uow);

        await handler.HandleAsync(new TicketCapacityAddedEvent
        {
            Id = ticketTypeId,
            EventId = eventId,
            AddedQuantity = 30
        });

        Assert.Equal(80, ticketType.Quantity);
        Assert.Equal(80, ticketType.Remaining);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_WhenAvailabilityNotFound_ThrowsNotFoundException()
    {
        var repo = new InMemoryTicketAvailabilityRepository();
        var uow = new CountingUnitOfWork();
        var handler = new TicketCapacityAddedEventHandler(repo, uow);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.HandleAsync(new TicketCapacityAddedEvent
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                AddedQuantity = 10
            }));
    }
}

// ════════════════════════════════════════════════════════════════════════
// TicketTypeDeletedEventHandler
// ════════════════════════════════════════════════════════════════════════

public class TicketTypeDeletedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_RemovesTicketTypeAndSaves()
    {
        var eventId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();
        var availability = new TicketAvailability(eventId, [new TicketTypeAvailability(ticketTypeId, 100)]);
        var repo = new InMemoryTicketAvailabilityRepository(availability);
        var uow = new CountingUnitOfWork();
        var handler = new TicketTypeDeletedEventHandler(repo, uow);

        await handler.HandleAsync(new TicketTypeDeletedEvent { Id = ticketTypeId, EventId = eventId });

        Assert.Empty(availability.TicketTypes);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_WhenAvailabilityNotFound_DoesNothing()
    {
        var repo = new InMemoryTicketAvailabilityRepository();
        var uow = new CountingUnitOfWork();
        var handler = new TicketTypeDeletedEventHandler(repo, uow);

        await handler.HandleAsync(new TicketTypeDeletedEvent { Id = Guid.NewGuid(), EventId = Guid.NewGuid() });

        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_WhenTicketTypeIdNotInAvailability_SavesWithoutRemoving()
    {
        var eventId = Guid.NewGuid();
        var availability = new TicketAvailability(eventId, [new TicketTypeAvailability(Guid.NewGuid(), 50)]);
        var repo = new InMemoryTicketAvailabilityRepository(availability);
        var uow = new CountingUnitOfWork();
        var handler = new TicketTypeDeletedEventHandler(repo, uow);

        await handler.HandleAsync(new TicketTypeDeletedEvent { Id = Guid.NewGuid(), EventId = eventId });

        Assert.Single(availability.TicketTypes); // unchanged
        Assert.Equal(1, uow.SaveCount);
    }
}

// ════════════════════════════════════════════════════════════════════════
// Shared test doubles
// ════════════════════════════════════════════════════════════════════════

file sealed class CountingUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public IOrderRepository Orders => throw new NotSupportedException();
    public ITicketAvailabilityRepository TicketAvailabilities => throw new NotSupportedException();
    public IEventSnapshotRepository EventSnapshots => throw new NotSupportedException();
    public ITicketRepository Tickets => throw new NotSupportedException();
    public IOutbox Outbox => throw new NotSupportedException();

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        SaveCount++;
        return Task.FromResult(1);
    }

    public Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken ct = default)
        => operation();
}

file sealed class StubOrderRepository(Order? order) : IOrderRepository
{
    public Task<Order?> GetByIdWithDetailsAsync(Guid orderId, CancellationToken ct = default)
        => Task.FromResult(order?.Id == orderId ? order : null);

    public void Update(Order entity) { }

    // ── unused stubs ──
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Order?>(null);
    public Task AddAsync(Order entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task AddRangeAsync(IEnumerable<Order> entities, CancellationToken ct = default) => Task.CompletedTask;
    public void Delete(Order entity) { }
    public IQueryable<Order> Get(Expression<Func<Order, bool>> f = null!, Func<IQueryable<Order>, IOrderedQueryable<Order>> o = null!, string i = null!, bool n = true) => Enumerable.Empty<Order>().AsQueryable();
    public Task<IEnumerable<Order>> GetByEventIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Enumerable.Empty<Order>());
    public Task<IEnumerable<Order>> GetByUserIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Enumerable.Empty<Order>());
    public Task<IEnumerable<Order>> GetByEmailAsync(string email, CancellationToken ct = default) => Task.FromResult(Enumerable.Empty<Order>());
    public Task<IEnumerable<Order>> GetByStateAsync(OrderState state, CancellationToken ct = default) => Task.FromResult(Enumerable.Empty<Order>());
    public Task<Order?> GetByAccessCodeAsync(string code, CancellationToken ct = default) => Task.FromResult<Order?>(null);
    public Task<bool> HasActivePendingOrderAsync(Guid eid, Guid? uid, string email, DateTimeOffset now, CancellationToken ct = default) => Task.FromResult(false);
}

file sealed class SpyEventSnapshotRepository : IEventSnapshotRepository
{
    public record UpsertCall(Guid EventId, DateTimeOffset? From, DateTimeOffset? To, bool? IsPublished);
    public List<UpsertCall> UpsertCalls { get; } = [];

    public Task<EventSnapshot?> GetByEventIdAsync(Guid eventId, CancellationToken ct = default)
        => Task.FromResult<EventSnapshot?>(null);

    public Task UpsertAsync(Guid eventId, DateTimeOffset? from, DateTimeOffset? to, bool? isPublished, CancellationToken ct = default)
    {
        UpsertCalls.Add(new(eventId, from, to, isPublished));
        return Task.CompletedTask;
    }
}

file sealed class InMemoryTicketAvailabilityRepository : ITicketAvailabilityRepository
{
    public Dictionary<Guid, TicketAvailability> Stored { get; } = new();

    public InMemoryTicketAvailabilityRepository() { }

    public InMemoryTicketAvailabilityRepository(TicketAvailability seed)
        => Stored[seed.Id] = seed;

    public Task<TicketAvailability?> GetByEventIdAsync(Guid eventId, CancellationToken ct = default)
        => Task.FromResult(Stored.GetValueOrDefault(eventId));

    public Task AddAsync(TicketAvailability entity, CancellationToken ct = default)
    {
        Stored[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<TicketAvailability> entities, CancellationToken ct = default)
    {
        foreach (var entity in entities)
            Stored[entity.Id] = entity;

        return Task.CompletedTask;
    }

    public void Update(TicketAvailability entity) { }

    // ── unused stubs ──
    public Task<TicketAvailability?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Stored.GetValueOrDefault(id));
    public void Delete(TicketAvailability entity) => Stored.Remove(entity.Id);
    public IQueryable<TicketAvailability> Get(Expression<Func<TicketAvailability, bool>> f = null!, Func<IQueryable<TicketAvailability>, IOrderedQueryable<TicketAvailability>> o = null!, string i = null!, bool n = true) => Stored.Values.AsQueryable();
    public Task<bool> TryReserveAsync(Guid eid, Guid tid, int qty, CancellationToken ct = default) => Task.FromResult(false);
}

file sealed class RecordingMediator : IMediator
{
    public List<object> SentRequests { get; } = [];

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        SentRequests.Add(request);
        return Task.FromResult(default(TResponse)!);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken ct = default) where TRequest : IRequest
    {
        SentRequests.Add(request!);
        return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken ct = default)
    {
        SentRequests.Add(request);
        return Task.FromResult<object?>(null);
    }

    public Task Publish(object notification, CancellationToken ct = default) => Task.CompletedTask;
    public Task Publish<TNotification>(TNotification notification, CancellationToken ct = default) where TNotification : INotification => Task.CompletedTask;
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken ct = default) => throw new NotSupportedException();
    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken ct = default) => throw new NotSupportedException();
}
