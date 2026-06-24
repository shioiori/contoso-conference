namespace Eventbox.Shared.Exceptions;

public sealed class UnauthorizedApiException(string message) : ApiException(message);
