namespace Eventbox.Shared.SeedWorks
{
    public abstract class AuditableEntity<T>(T id) : Entity<T>(id), IAuditableEntity
    {
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

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
