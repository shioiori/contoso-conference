namespace Eventbox.Shared.Exceptions;

public sealed class ForbiddenApiException(string message) : ApiException(message);
