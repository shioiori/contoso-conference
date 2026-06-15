namespace Eventbox.Shared.Exceptions;

public abstract class ApiException : Exception
{
    protected ApiException(
        string message,
        int statusCode,
        string title,
        string errorCode,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Title = title;
        ErrorCode = errorCode;
    }

    public int StatusCode { get; }

    public string Title { get; }

    public string ErrorCode { get; }
}
