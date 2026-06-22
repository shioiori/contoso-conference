namespace Eventbox.Shared.Exceptions;

public abstract class ApiException(string message, Exception? innerException = null)
    : Exception(message, innerException);
