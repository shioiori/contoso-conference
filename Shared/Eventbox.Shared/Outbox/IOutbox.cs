namespace Eventbox.Shared.Outbox
{
    public interface IOutbox
    {
        Task AddAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken);
        void Update(OutboxMessage outboxMessage);
        Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken);
    }
}
