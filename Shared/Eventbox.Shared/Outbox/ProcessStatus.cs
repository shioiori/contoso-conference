namespace Eventbox.Shared.Outbox
{
    public enum ProcessStatus
    {
        Pending,
        Processing,
        Processed,
        Failed
    }
}
