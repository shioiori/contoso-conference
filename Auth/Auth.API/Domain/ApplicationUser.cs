using Eventbox.Shared.Auditing;
using Microsoft.AspNetCore.Identity;

namespace Eventbox.Auth.Api.Domain;

public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity
{
    public AccountType AccountType { get; set; }
    public DateTimeOffset CreatedDate { get; private set; }
    public DateTimeOffset? UpdatedDate { get; private set; }
    public string? CreatedBy { get; private set; }
    public string? UpdatedBy { get; private set; }

    public void MarkCreated(string? userId, DateTimeOffset utcNow)
    {
        CreatedDate = utcNow;
        CreatedBy = userId;
        UpdatedDate = null;
        UpdatedBy = null;
    }

    public void MarkUpdated(string? userId, DateTimeOffset utcNow)
    {
        UpdatedDate = utcNow;
        UpdatedBy = userId;
    }
}
