using MediatR;
using Microsoft.Extensions.Logging;

namespace Eventbox.Shared.Auditing;

public sealed class AuditLoggingBehavior<TRequest, TResponse>(
    ILogger<AuditLoggingBehavior<TRequest, TResponse>> logger,
    IAuditContextAccessor auditContextAccessor)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        if (!requestName.EndsWith("Command", StringComparison.Ordinal))
        {
            return await next(cancellationToken);
        }

        var context = auditContextAccessor.GetCurrent();
        var payload = AuditSerialization.Serialize(AuditRedactor.RedactObject(request));

        try
        {
            var response = await next(cancellationToken);

            using (logger.BeginScope(new Dictionary<string, object?>
            {
                ["OrganizationId"] = context.OrganizationId,
                ["UserId"] = context.UserId,
                ["TraceId"] = context.TraceId,
                ["AuditKind"] = "MediatRCommand",
                ["CommandName"] = requestName
            }))
            {
                logger.LogInformation("Business audit command {CommandName} completed with payload {Payload}", requestName, payload);
            }

            return response;
        }
        catch (Exception ex)
        {
            using (logger.BeginScope(new Dictionary<string, object?>
            {
                ["OrganizationId"] = context.OrganizationId,
                ["UserId"] = context.UserId,
                ["TraceId"] = context.TraceId,
                ["AuditKind"] = "MediatRCommand",
                ["CommandName"] = requestName
            }))
            {
                logger.LogWarning(ex, "Business audit command {CommandName} failed with payload {Payload}", requestName, payload);
            }

            throw;
        }
    }
}
