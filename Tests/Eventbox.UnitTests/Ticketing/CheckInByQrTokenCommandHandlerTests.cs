using System.Linq.Expressions;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.Commands;
using Eventbox.Ticketing.Domain.Entities;
using Eventbox.Ticketing.Domain.Entities.OrderAggregate;
using Eventbox.Ticketing.Domain.Enums;

namespace Eventbox.UnitTests.Registration;

public class CheckInByQrTokenCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTwoRequestsUseSameQrToken_OnlyOneCheckInSucceeds()
    {
        var eventId = Guid.NewGuid();
        const string qrToken = "qr-token";
        var ticket = new Ticket(eventId, ticketTypeId: 7, sequenceNumber: 1);
        ticket.AssignQrToken(qrToken, qrToken);

        var orderRepository = new InMemoryOrderRepository(ticket);
        var scheduleRepository = new InMemoryEventScheduleRepository(
            new EventSchedule(eventId, DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddMinutes(5)));
        var handler = new CheckInByQrTokenCommandHandler(orderRepository, scheduleRepository, new PassthroughQrTokenHasher());

        var command = new CheckInByQrTokenCommand
        {
            EventId = eventId,
            QrToken = qrToken,
            StaffUserId = Guid.NewGuid()
        };

        var results = await Task.WhenAll(
            handler.Handle(command, CancellationToken.None),
            handler.Handle(command, CancellationToken.None));

        Assert.Single(results, result => result.Result == CheckInAttemptResult.Success);
        Assert.Single(results, result => result.Result == CheckInAttemptResult.AlreadyCheckedIn);
        Assert.NotNull(ticket.CheckedInAt);
    }

    private sealed class PassthroughQrTokenHasher : IQrTokenHasher
    {
        public string Hash(string qrToken) => qrToken;
    }

    private sealed class InMemoryEventScheduleRepository(EventSchedule schedule) : IEventScheduleRepository
    {
        public Task<EventSchedule?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
            => Task.FromResult(eventId == schedule.Id ? schedule : null);

        public Task UpsertAsync(Guid eventId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class InMemoryOrderRepository(Ticket ticket) : IOrderRepository
    {
        private readonly object _gate = new();

        public Task<Ticket?> GetTicketByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default)
            => Task.FromResult(ticket.QrTokenHash == qrTokenHash ? ticket : null);

        public Task<int> TryMarkTicketCheckedInAsync(Guid ticketId, Guid? staffUserId, DateTimeOffset checkedInAt, CancellationToken cancellationToken = default)
        {
            lock (_gate)
            {
                if (ticket.Id != ticketId || ticket.TicketState != TicketState.Active || ticket.CheckedInAt.HasValue)
                    return Task.FromResult(0);

                ticket.CheckIn(staffUserId ?? Guid.Empty, checkedInAt);
                return Task.FromResult(1);
            }
        }

        public Task<bool> ExistsTicketByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default)
            => Task.FromResult(ticket.QrTokenHash == qrTokenHash);

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Order?>(null);

        public Task AddAsync(Order entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Update(Order entity)
        {
        }

        public void Delete(Order entity)
        {
        }

        public IQueryable<Order> Get(
            Expression<Func<Order, bool>> filter = null!,
            Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = null!,
            string includeProperties = null!,
            bool needAsNoTracking = true)
            => Array.Empty<Order>().AsQueryable();

        public Task<IEnumerable<Order>> GetByEventIdAsync(Guid EventId, CancellationToken cancellationToken = default)
            => Task.FromResult(Enumerable.Empty<Order>());

        public Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(Enumerable.Empty<Order>());

        public Task<IEnumerable<Order>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(Enumerable.Empty<Order>());

        public Task<IEnumerable<Order>> GetByStateAsync(OrderState state, CancellationToken cancellationToken = default)
            => Task.FromResult(Enumerable.Empty<Order>());

        public Task<Order?> GetByIdWithDetailsAsync(Guid orderId, CancellationToken cancellationToken = default)
            => Task.FromResult<Order?>(null);

        public Task<Order?> GetByAccessCodeAsync(string accessCode, CancellationToken cancellationToken = default)
            => Task.FromResult<Order?>(null);

        public Task<bool> HasActivePendingOrderAsync(Guid eventId, Guid? userId, string email, DateTimeOffset utcNow, CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }
}
