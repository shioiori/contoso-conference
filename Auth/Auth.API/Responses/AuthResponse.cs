namespace Eventbox.Auth.Api.Responses;

public class AuthResponse
{
    public string AccessToken { get; set; } 
    public DateTimeOffset ExpiresAt { get; set; }
}
