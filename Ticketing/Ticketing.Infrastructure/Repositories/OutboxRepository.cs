using Eventbox.Shared.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.Ticketing.Infrastructure
{
    public class OutboxRepository(TicketingDbContext dbContext) : IOutbox
    {
        public async Task AddAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
            => await dbContext.Outboxes.AddAsync(outboxMessage, cancellationToken);

        public void Update(OutboxMessage outboxMessage)
            => dbContext.Outboxes.Update(outboxMessage);

        public Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken)
            => dbContext.Outboxes
                .Where(x => x.Status == ProcessStatus.Pending
                    || (x.Status == ProcessStatus.Failed && x.RetryCount < OutboxMessage.MaxRetries))
                .OrderBy(x => x.OccurredOnUtc)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
    }
}
