using Eventbox.Auth.Api.Domain;
using Eventbox.Auth.Api.Requests;
using Eventbox.Auth.Api.Services;
using Eventbox.Shared.Constants;
using Eventbox.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Eventbox.Auth.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/customers/register", (
            RegisterRequest request,
            UserManager<ApplicationUser> userManager,
            TokenService tokenService) => RegisterAsync(request, AccountType.Customer, userManager, tokenService));

        app.MapPost("/api/auth/organizers/register", (
            RegisterRequest request,
            UserManager<ApplicationUser> userManager,
            TokenService tokenService) => RegisterAsync(request, AccountType.Organizer, userManager, tokenService));

        app.MapPost("/api/auth/customers/login", (
            LoginRequest request,
            UserManager<ApplicationUser> userManager,
            TokenService tokenService) => LoginAsync(request, AccountType.Customer, userManager, tokenService));

        app.MapPost("/api/auth/organizers/login", (
            LoginRequest request,
            UserManager<ApplicationUser> userManager,
            TokenService tokenService) => LoginAsync(request, AccountType.Organizer, userManager, tokenService));

        app.MapGet("/api/auth/me", (HttpContext httpContext) =>
        {
            if (httpContext.User.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedApiException("Authentication is required.");
            }

            var id = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var accountType = httpContext.User.FindFirst(CustomClaimTypes.AccountType)?.Value;

            return Results.Ok(new { id, email, accountType });
        }).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        AccountType accountType,
        UserManager<ApplicationUser> userManager,
        TokenService tokenService)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password) || user.AccountType != accountType)
        {
            throw new UnauthorizedApiException("Invalid email or password.");
        }

        return Results.Ok(tokenService.CreateToken(user));
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        AccountType accountType,
        UserManager<ApplicationUser> userManager,
        TokenService tokenService)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            AccountType = accountType
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new ValidationApiException(new Dictionary<string, string[]>
            {
                ["identity"] = result.Errors.Select(e => e.Description).ToArray()
            });
        }

        return Results.Ok(tokenService.CreateToken(user));
    }
}
