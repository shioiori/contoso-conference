using Eventbox.Ticketing.Api.Options;
using Microsoft.Extensions.Options;

namespace Eventbox.Ticketing.Api.Security;

public sealed class InternalServiceTokenEndpointFilter(IOptions<InternalApiOptions> options) : IEndpointFilter
{
    public const string HeaderName = "X-Internal-Service-Token";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var configuredToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(configuredToken))
        {
            return Results.Problem(
                title: "Internal API token is not configured.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var httpContext = context.HttpContext;
        if (!httpContext.Request.Headers.TryGetValue(HeaderName, out var token) ||
            !string.Equals(token.ToString(), configuredToken, StringComparison.Ordinal))
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}
