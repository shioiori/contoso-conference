namespace Eventbox.Payment.Core.Abstractions
{
    public interface IOutboxProcessorJob
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
