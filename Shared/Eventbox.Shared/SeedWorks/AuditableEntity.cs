namespace Eventbox.Shared.SeedWorks
{
    public abstract class AuditableEntity<T> : Entity<T>, IAuditableEntity
    {
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        protected AuditableEntity() { }
        protected AuditableEntity(T id) : base(id) { }

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
