namespace Eventbox.Auth.Api.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "Eventbox.Auth";
    public string Audience { get; init; } = "Eventbox.Api";
    public string SigningKey { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 120;
}
