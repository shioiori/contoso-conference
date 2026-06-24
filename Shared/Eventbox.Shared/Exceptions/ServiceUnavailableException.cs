namespace Eventbox.Shared.Exceptions;

public sealed class ServiceUnavailableException(string message) : ApiException(message);
