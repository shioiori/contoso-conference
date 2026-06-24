namespace Eventbox.Shared.Exceptions;

public sealed class ValidationApiException : ApiException
{
    public ValidationApiException(string message) : base(message)
    {
    }

    public ValidationApiException(IReadOnlyDictionary<string, string[]> errors)
        : this("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]>? Errors { get; }
}
