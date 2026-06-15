using Microsoft.AspNetCore.Http;

namespace Eventbox.Shared.Exceptions;

public sealed class ConflictException : ApiException
{
    public ConflictException(string message)
        : base(message, StatusCodes.Status409Conflict, "Conflict", "conflict")
    {
    }
}
