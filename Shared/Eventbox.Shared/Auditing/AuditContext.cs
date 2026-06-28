namespace Eventbox.Shared.Auditing;

public sealed class AuditContext
{
    public string OrganizationId { get; set; }
    public string UserId { get; set; }
    public string TraceId { get; set; }
}

