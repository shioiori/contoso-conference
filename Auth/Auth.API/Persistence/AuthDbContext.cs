using Eventbox.Auth.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.Auth.Api.Persistence;

public class AuthDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }
}
