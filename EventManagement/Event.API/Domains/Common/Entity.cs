namespace Eventbox.EventManagement.EventApi.Domains.Common
{
    public abstract class Entity<TId>
    {
        public TId Id { get; protected set; }
        public DateOnly CreatedAt { get; protected set; }
        public DateOnly? UpdatedAt { get; protected set; }
        public string? CreatedBy { get; protected set; }
        public string? UpdatedBy { get; protected set; }

        protected Entity(TId id)
        {
            Id = id;
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        protected Entity()
        {
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow);
        }
}
}
