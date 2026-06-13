using Eventbox.Auth.Api.Domain;
using Eventbox.Auth.Api.Requests;
using Eventbox.Auth.Api.Services;
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

        app.MapPost("/api/auth/login", async (
            LoginRequest request,
            UserManager<ApplicationUser> userManager,
            TokenService tokenService) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                return Results.Unauthorized();
            }

            return Results.Ok(tokenService.CreateToken(user));
        });

        app.MapGet("/api/auth/me", (HttpContext httpContext) =>
        {
            if (httpContext.User.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var id = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var accountType = httpContext.User.FindFirst("account_type")?.Value;

            return Results.Ok(new { id, email, accountType });
        }).RequireAuthorization();

        return app;
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
            return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        return Results.Ok(tokenService.CreateToken(user));
    }
}
