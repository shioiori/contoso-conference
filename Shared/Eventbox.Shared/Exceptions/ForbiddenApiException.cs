using Microsoft.AspNetCore.Http;

namespace Eventbox.Shared.Exceptions;

public sealed class ForbiddenApiException : ApiException
{
    public ForbiddenApiException(string message)
        : base(message, StatusCodes.Status403Forbidden, "Forbidden", "forbidden")
    {
    }
}
