using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Eventbox.EventManagement.EventApi.Api.Authorization;

public class OrganizationMemberRequirement : IAuthorizationRequirement { }

public class OrganizationMemberHandler(IUnitOfWork unitOfWork)
    : AuthorizationHandler<OrganizationMemberRequirement, Guid>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganizationMemberRequirement requirement,
        Guid organizationId)
    {
        var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return;

        var isMember = await unitOfWork.OrganizationMembers.ExistsAsync(organizationId, userId);
        if (isMember)
            context.Succeed(requirement);
    }
}
