namespace Eventbox.Shared.Auditing;

public interface IAuditableEntity
{
    DateTimeOffset CreatedDate { get; }
    DateTimeOffset? UpdatedDate { get; }
    string? CreatedBy { get; }
    string? UpdatedBy { get; }

    void MarkCreated(string? userId, DateTimeOffset utcNow);
    void MarkUpdated(string? userId, DateTimeOffset utcNow);
}
