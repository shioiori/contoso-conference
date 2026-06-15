using Microsoft.AspNetCore.Http;

namespace Eventbox.Shared.Exceptions;

public sealed class NotFoundException : ApiException
{
    public NotFoundException(string message)
        : base(message, StatusCodes.Status404NotFound, "Resource not found", "not-found")
    {
    }

    public NotFoundException(string resourceName, object resourceId)
        : this($"{resourceName} '{resourceId}' was not found.")
    {
    }
}
