namespace Eventbox.Shared.Auditing;

public interface IAuditContextAccessor
{
    AuditContext GetCurrent();
}
