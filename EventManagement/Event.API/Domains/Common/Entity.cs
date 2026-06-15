using Eventbox.Shared.Auditing;

namespace Eventbox.EventManagement.EventApi.Domains.Common
{
    public abstract class Entity<TId> : IAuditableEntity
    {
        public TId Id { get; protected set; }
        public DateTimeOffset CreatedDate { get; private set; }
        public DateTimeOffset? UpdatedDate { get; private set; }
        public string? CreatedBy { get; protected set; }
        public string? UpdatedBy { get; protected set; }

        protected Entity(TId id)
        {
            Id = id;
        }

        protected Entity()
        {
        }

        public void MarkCreated(string? userId, DateTimeOffset utcNow)
        {
            CreatedDate = utcNow;
            CreatedBy = userId;
            UpdatedDate = null;
            UpdatedBy = null;
        }

        public void MarkUpdated(string? userId, DateTimeOffset utcNow)
        {
            UpdatedDate = utcNow;
            UpdatedBy = userId;
        }
}
}
