using Eventbox.EventManagement.EventApi.Api.Authorization;
using Eventbox.Shared.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Eventbox.EventManagement.EventApi.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];
        var jwtSigningKey = configuration["Jwt:SigningKey"];

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

        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyName.RequireOrganizerAccount, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(CustomClaimTypes.AccountType, AccountTypes.Organizer));

            options.AddPolicy(PolicyName.RequireOrganizationMember, policy =>
                policy.RequireAuthenticatedUser()
                    .AddRequirements(new OrganizationMemberRequirement()));
        });

        services.AddScoped<IAuthorizationHandler, OrganizationMemberHandler>();

        return services;
    }
}
