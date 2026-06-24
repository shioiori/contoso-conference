namespace Eventbox.Shared.SeedWorks
{
    public interface IDomainEvent
    {
        public Guid Id { get; set; }
        public DateOnly Timestamp { get; set; }
    }
}
