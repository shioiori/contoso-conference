namespace Eventbox.Shared.Exceptions;

public sealed class NotFoundException(string message) : ApiException(message)
{
    public NotFoundException(string resourceName, object resourceId)
        : this($"{resourceName} '{resourceId}' was not found.")
    {
    }
}
