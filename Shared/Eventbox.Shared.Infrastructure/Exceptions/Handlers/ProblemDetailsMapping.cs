using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eventbox.Shared.Exceptions.Handlers;

internal static class ProblemDetailsMapping
{
    private const string ProblemTypePrefix = "/problems/";

    public static ProblemDetails ToProblemDetails(
        this Exception exception,
        PathString path,
        bool includeExceptionDetails)
    {
        return exception switch
        {
            ValidationApiException { Errors: not null } ex => CreateValidationProblemDetails(ex, path),
            ValidationApiException ex => Create(StatusCodes.Status400BadRequest, "Validation failed", ex.Message, "validation", path),
            ConflictException ex => Create(StatusCodes.Status409Conflict, "Conflict", ex.Message, "conflict", path),
            NotFoundException ex => Create(StatusCodes.Status404NotFound, "Resource not found", ex.Message, "not-found", path),
            ForbiddenApiException ex => Create(StatusCodes.Status403Forbidden, "Forbidden", ex.Message, "forbidden", path),
            UnauthorizedApiException ex => Create(StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message, "unauthorized", path),
            ServiceUnavailableException ex => Create(StatusCodes.Status503ServiceUnavailable, "Service unavailable", ex.Message, "service-unavailable", path),
            BadHttpRequestException ex => Create(ex.StatusCode, "Bad request", ex.Message, "bad-request", path),
            KeyNotFoundException ex => Create(StatusCodes.Status404NotFound, "Resource not found", ex.Message, "not-found", path),
            ArgumentException ex => Create(StatusCodes.Status400BadRequest, "Invalid request", ex.Message, "invalid-request", path),
            InvalidOperationException ex => Create(StatusCodes.Status400BadRequest, "Invalid operation", ex.Message, "invalid-operation", path),
            _ => Create(
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                includeExceptionDetails ? exception.Message : "An unexpected error occurred.",
                "internal-error",
                path)
        };
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        ValidationApiException exception,
        PathString path)
        => new(exception.Errors!.ToDictionary(
            pair => pair.Key,
            pair => pair.Value))
        {
            Type = $"{ProblemTypePrefix}validation",
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = exception.Message,
            Instance = path
        };

    private static ProblemDetails Create(
        int status,
        string title,
        string detail,
        string errorCode,
        PathString path)
        => new()
        {
            Type = $"{ProblemTypePrefix}{errorCode}",
            Title = title,
            Status = status,
            Detail = detail,
            Instance = path
        };
}
