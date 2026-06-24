namespace Eventbox.Shared.Exceptions;

public sealed class ConflictException(string message) : ApiException(message);
