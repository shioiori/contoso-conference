using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eventbox.Shared.Exceptions.Handlers;

internal static class ProblemDetailsMapping
{
    public static ProblemDetails ToProblemDetails(
        this Exception exception,
        PathString path,
        bool includeExceptionDetails)
    {
        var problemDetails = exception switch
        {
            ValidationApiException { Errors: not null } validationException => CreateValidationProblemDetails(
                validationException,
                path),
            ApiException apiException => Create(
                apiException.StatusCode,
                apiException.Title,
                apiException.Message,
                apiException.ErrorCode,
                path),
            KeyNotFoundException keyNotFoundException => Create(
                StatusCodes.Status404NotFound,
                "Resource not found",
                keyNotFoundException.Message,
                "not-found",
                path),
            ArgumentException argumentException => Create(
                StatusCodes.Status400BadRequest,
                "Invalid request",
                argumentException.Message,
                "invalid-request",
                path),
            InvalidOperationException invalidOperationException => Create(
                StatusCodes.Status400BadRequest,
                "Invalid operation",
                invalidOperationException.Message,
                "invalid-operation",
                path),
            _ => Create(
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                includeExceptionDetails
                    ? exception.Message
                    : "An unexpected error occurred.",
                "internal-error",
                path)
        };

        return problemDetails;
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        ValidationApiException exception,
        PathString path)
        => new(exception.Errors!.ToDictionary(
            pair => pair.Key,
            pair => pair.Value))
        {
            Type = $"https://eventbox.dev/problems/{exception.ErrorCode}",
            Title = exception.Title,
            Status = exception.StatusCode,
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
            Type = $"https://eventbox.dev/problems/{errorCode}",
            Title = title,
            Status = status,
            Detail = detail,
            Instance = path
        };
}
