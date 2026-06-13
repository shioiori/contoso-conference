namespace Eventbox.Auth.Api.Responses;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
}
