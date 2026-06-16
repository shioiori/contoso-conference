using System.Linq.Expressions;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Commands;
using Eventbox.Ticketing.Application.Dtos;
using Eventbox.Ticketing.Domain.Entities.OrderAggregate;
using Eventbox.Ticketing.Domain.Entities.TicketAvailabilityAggregate;
using Eventbox.Ticketing.Domain.Enums;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.Abstractions.Jobs;
using Eventbox.Shared.Exceptions;

namespace Eventbox.UnitTests.Registration;

public class RegisterToEventCommandHandlerConcurrencyTests
{
    [Fact]
    public async Task RegisterToEventAsync_WhenTwoRequestsRaceForLastTicket_DoesNotOversell()
    {
        var eventId = Guid.NewGuid();
        var ticketType = new TicketTypeAvailability(ticketTypeId: 7, quantity: 1);
        var availability = new TicketAvailability(eventId, [ticketType]);
        var orderRepository = new InMemoryOrderRepository();
        var ticketAvailabilityRepository = new InMemoryTicketAvailabilityRepository(availability);
        var scheduler = new RecordingOrderExpirationScheduler();
        var unitOfWork = new InMemoryRegistrationUnitOfWork(orderRepository, ticketAvailabilityRepository);
        var handler = new RegisterToEventCommandHandler(
            scheduler,
            unitOfWork);

        var firstRequest = CreateRequest(eventId, "first@example.com");
        var secondRequest = CreateRequest(eventId, "second@example.com");

        var attempts = await Task.WhenAll(
            CaptureResult(() => handler.Handle(firstRequest, CancellationToken.None)),
            CaptureResult(() => handler.Handle(secondRequest, CancellationToken.None)));

        Assert.Single(attempts, attempt => attempt.Order is not null);
        Assert.Single(attempts, attempt => attempt.Exception is ConflictException);
        Assert.Equal(0, ticketType.Remaining);
        Assert.Single(orderRepository.Orders);
        Assert.Single(scheduler.ScheduledOrderIds);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    private static RegisterToEventCommand CreateRequest(Guid eventId, string email)
        => new()
        {
            EventId = eventId,
            Name = "Buyer",
            Email = email,
            TicketTypeId = 7,
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

    private sealed class InMemoryTicketAvailabilityRepository(TicketAvailability availability) : ITicketAvailabilityRepository
    {
        private readonly object _gate = new();

        public Task<TicketAvailability?> GetByEventIdAsync(Guid EventId, CancellationToken cancellationToken = default)
            => Task.FromResult(EventId == availability.Id ? availability : null);

        public Task<bool> TryReserveAsync(Guid eventId, int ticketTypeId, int quantity, CancellationToken cancellationToken = default)
        {
            lock (_gate)
            {
                if (eventId != availability.Id)
                    return Task.FromResult(false);

                try
                {
                    availability.Reserve(ticketTypeId, quantity);
                    return Task.FromResult(true);
                }
                catch (InvalidOperationException)
                {
                    return Task.FromResult(false);
                }
            }
        }

        public Task<TicketAvailability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => GetByEventIdAsync(id, cancellationToken);

        public Task AddAsync(TicketAvailability entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Update(TicketAvailability entity)
        {
        }

        public void Delete(TicketAvailability entity)
        {
        }

        public IQueryable<TicketAvailability> Get(
            Expression<Func<TicketAvailability, bool>> filter = null!,
            Func<IQueryable<TicketAvailability>, IOrderedQueryable<TicketAvailability>> orderBy = null!,
            string includeProperties = null!,
            bool needAsNoTracking = true)
        {
            var query = new[] { availability }.AsQueryable();
            return filter is null ? query : query.Where(filter);
        }
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

        public void Update(Order entity)
        {
        }

        public void Delete(Order entity)
            => _orders.Remove(entity);

        public IQueryable<Order> Get(
            Expression<Func<Order, bool>> filter = null!,
            Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = null!,
            string includeProperties = null!,
            bool needAsNoTracking = true)
        {
            var query = _orders.AsQueryable();
            return filter is null ? query : query.Where(filter);
        }

        public Task<IEnumerable<Order>> GetByEventIdAsync(Guid EventId, CancellationToken cancellationToken = default)
            => Task.FromResult(_orders.Where(order => order.EventId == EventId));

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

        public Task<Ticket?> GetTicketByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default)
            => Task.FromResult(_orders.SelectMany(order => order.Tickets).FirstOrDefault(ticket => ticket.QrTokenHash == qrTokenHash));

        public Task<bool> ExistsTicketByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default)
            => Task.FromResult(_orders.SelectMany(order => order.Tickets).Any(ticket => ticket.QrTokenHash == qrTokenHash));

        public Task<int> TryMarkTicketCheckedInAsync(Guid ticketId, Guid? staffUserId, DateTimeOffset checkedInAt, CancellationToken cancellationToken = default)
        {
            var ticket = _orders.SelectMany(order => order.Tickets).FirstOrDefault(ticket => ticket.Id == ticketId);
            if (ticket is null || ticket.TicketState != TicketState.Active || ticket.CheckedInAt.HasValue)
                return Task.FromResult(0);

            ticket.CheckIn(staffUserId ?? Guid.Empty, checkedInAt);
            return Task.FromResult(1);
        }
    }

    private sealed class InMemoryRegistrationUnitOfWork(
        IOrderRepository orderRepository,
        ITicketAvailabilityRepository ticketAvailabilityRepository) : IUnitOfWork
    {
        public IOrderRepository Orders { get; } = orderRepository;
        public ITicketAvailabilityRepository TicketAvailabilities { get; } = ticketAvailabilityRepository;

        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }

        public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
            => await operation();
    }

    private sealed class RecordingOrderExpirationScheduler : IOrderExpirationScheduler
    {
        private readonly List<Guid> _scheduledOrderIds = new();

        public IReadOnlyCollection<Guid> ScheduledOrderIds => _scheduledOrderIds.AsReadOnly();

        public Task ScheduleExpirationAsync(Guid orderId, DateTimeOffset expiresAt, CancellationToken cancellationToken = default)
        {
            _scheduledOrderIds.Add(orderId);
            return Task.CompletedTask;
        }
    }
}
