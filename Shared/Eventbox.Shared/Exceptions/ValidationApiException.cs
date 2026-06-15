using Microsoft.AspNetCore.Http;

namespace Eventbox.Shared.Exceptions;

public sealed class ValidationApiException : ApiException
{
    public ValidationApiException(string message)
        : base(message, StatusCodes.Status400BadRequest, "Validation failed", "validation")
    {
    }

    public ValidationApiException(IReadOnlyDictionary<string, string[]> errors)
        : this("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]>? Errors { get; }
}
