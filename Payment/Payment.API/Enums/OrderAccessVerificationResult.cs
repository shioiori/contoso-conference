namespace Eventbox.Payment.Api.Enums;

public enum OrderAccessVerificationResult
{
    Authorized,
    Unauthorized,
    Forbidden,
    NotFound,
    RegistrationUnavailable
}
