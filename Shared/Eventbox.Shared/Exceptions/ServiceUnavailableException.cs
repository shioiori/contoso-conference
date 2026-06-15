using Microsoft.AspNetCore.Http;

namespace Eventbox.Shared.Exceptions;

public sealed class ServiceUnavailableException : ApiException
{
    public ServiceUnavailableException(string message)
        : base(
            message,
            StatusCodes.Status503ServiceUnavailable,
            "Service unavailable",
            "service-unavailable")
    {
    }
}
