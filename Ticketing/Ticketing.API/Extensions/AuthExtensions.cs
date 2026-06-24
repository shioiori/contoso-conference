using Eventbox.Shared.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Eventbox.Ticketing.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "Eventbox.Auth";
        var jwtAudience = configuration["Jwt:Audience"] ?? "Eventbox.Api";
        var jwtSigningKey = configuration["Jwt:SigningKey"] ?? "eventbox-development-signing-key-change-me";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
                    ValidateLifetime = true
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyName.RequireCustomerAccount, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(CustomClaimTypes.AccountType, AccountTypes.Customer))
            .AddPolicy(PolicyName.RequireOrganizerAccount, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(CustomClaimTypes.AccountType, AccountTypes.Organizer));

        return services;
    }
}
