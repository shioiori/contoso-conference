using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Eventbox.Shared.Auditing;

public sealed class HttpAuditContextAccessor(IHttpContextAccessor httpContextAccessor) : IAuditContextAccessor
{
    public AuditContext GetCurrent()
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null)
        {
            return new AuditContext(null, null, null);
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");

        var organizationId = GetRouteValue(context, "organizationId")
            ?? GetRouteValue(context, "orgId")
            ?? context.Request.Headers["X-Organization-Id"].FirstOrDefault();

        return new AuditContext(
            Normalize(organizationId),
            Normalize(userId),
            Normalize(context.TraceIdentifier));
    }

    private static string? GetRouteValue(HttpContext context, string key)
        => context.Request.RouteValues.TryGetValue(key, out var value)
            ? value?.ToString()
            : null;

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}
