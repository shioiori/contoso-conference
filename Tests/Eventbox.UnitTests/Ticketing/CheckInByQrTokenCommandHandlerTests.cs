using System.Linq.Expressions;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.Commands;
using Eventbox.Ticketing.Domain.Entities;
using Eventbox.Ticketing.Domain.Enums;
using Eventbox.Ticketing.Domain.Tickets;

namespace Eventbox.UnitTests.Ticketing;

public class CheckInByQrTokenCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTwoRequestsUseSameQrToken_OnlyOneCheckInSucceeds()
    {
        var eventId = Guid.NewGuid();
        const string qrToken = "qr-token";
        var ticket = new Ticket(Guid.NewGuid(), eventId, ticketTypeId: Guid.NewGuid(), sequenceNumber: 1);
        ticket.AssignQrToken(qrToken, qrToken);

        var ticketRepository = new InMemoryTicketRepository(ticket);
        var snapshotRepository = new InMemoryEventSnapshotRepository(
            new EventSnapshot(eventId, DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddMinutes(5), isPublished: true));
        var handler = new CheckInByQrTokenCommandHandler(ticketRepository, snapshotRepository, new PassthroughQrTokenHasher());

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

    private sealed class InMemoryEventSnapshotRepository(EventSnapshot snapshot) : IEventSnapshotRepository
    {
        public Task<EventSnapshot?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
            => Task.FromResult(eventId == snapshot.Id ? snapshot : null);

        public Task UpsertAsync(Guid eventId, DateTimeOffset? from, DateTimeOffset? to, bool? isPublished, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class InMemoryTicketRepository(Ticket ticket) : ITicketRepository
    {
        private readonly object _gate = new();

        public Task<Ticket?> GetByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default)
            => Task.FromResult(ticket.QrTokenHash == qrTokenHash ? ticket : null);

        public Task<int> TryMarkCheckedInAsync(Guid ticketId, Guid? staffUserId, DateTimeOffset checkedInAt, CancellationToken cancellationToken = default)
        {
            lock (_gate)
            {
                if (ticket.Id != ticketId || ticket.TicketState != TicketState.Active || ticket.CheckedInAt.HasValue)
                    return Task.FromResult(0);

                ticket.CheckIn(staffUserId ?? Guid.Empty, checkedInAt);
                return Task.FromResult(1);
            }
        }

        public Task<bool> ExistsByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default)
            => Task.FromResult(ticket.QrTokenHash == qrTokenHash);

        public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(ticket.Id == id ? ticket : null);

        public Task AddAsync(Ticket entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task AddRangeAsync(IEnumerable<Ticket> entities, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Update(Ticket entity)
        {
        }

        public void Delete(Ticket entity)
        {
        }

        public IQueryable<Ticket> Get(
            Expression<Func<Ticket, bool>> filter = null!,
            Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>> orderBy = null!,
            string includeProperties = null!,
            bool needAsNoTracking = true)
            => new[] { ticket }.AsQueryable();

        public Task<IReadOnlyList<Ticket>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Ticket>>(ticket.OrderId == orderId ? [ticket] : []);

        public Task<IReadOnlyList<Ticket>> GetByOrderIdsAsync(IEnumerable<Guid> orderIds, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Ticket>>(orderIds.Contains(ticket.OrderId) ? [ticket] : []);
    }
}
