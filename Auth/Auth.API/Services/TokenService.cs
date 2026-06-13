using Eventbox.Auth.Api.Domain;
using Eventbox.Auth.Api.Options;
using Eventbox.Auth.Api.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Eventbox.Auth.Api.Services;

public class TokenService(IOptions<JwtOptions> jwtOptions)
{
    public AuthResponse CreateToken(ApplicationUser user)
    {
        var options = jwtOptions.Value;
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(options.ExpirationMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("account_type", user.AccountType.ToString())
        };

        var token = new JwtSecurityToken(
            options.Issuer,
            options.Audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AuthResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }
}
