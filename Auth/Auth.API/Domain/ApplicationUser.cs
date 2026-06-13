using Microsoft.AspNetCore.Identity;

namespace Eventbox.Auth.Api.Domain;

public class ApplicationUser : IdentityUser<Guid>
{
    public AccountType AccountType { get; set; }
}
