namespace Eventbox.Shared.Outbox
{
    public interface IOutboxProcessorJob
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
