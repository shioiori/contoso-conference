using Microsoft.AspNetCore.Http;

namespace Eventbox.Shared.Exceptions;

public sealed class UnauthorizedApiException : ApiException
{
    public UnauthorizedApiException(string message)
        : base(message, StatusCodes.Status401Unauthorized, "Unauthorized", "unauthorized")
    {
    }
}
