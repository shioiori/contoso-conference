namespace Eventbox.Shared.Auditing;

public sealed record AuditContext(
    string? OrganizationId,
    string? UserId,
    string? TraceId);
