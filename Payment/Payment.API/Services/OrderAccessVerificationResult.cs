namespace Eventbox.Payment.Api.Services;

public enum OrderAccessVerificationResult
{
    Authorized,
    Unauthorized,
    Forbidden,
    NotFound,
    RegistrationUnavailable
}
