using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Eventbox.Shared.Auditing;

public sealed class SerilogPropertiesMiddleware(IAuditContextAccessor auditContextAccessor) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var auditContext = auditContextAccessor.GetCurrent();

        using (LogContext.PushProperty("OrganizationId", auditContext.OrganizationId ?? string.Empty))
        using (LogContext.PushProperty("UserId", auditContext.UserId ?? string.Empty))
        using (LogContext.PushProperty("TraceId", auditContext.TraceId ?? string.Empty))
        {
            await next(context);
        }
    }
}
