using Eventbox.EventManagement.EventApi.Application.Abstractions.Services;
using Eventbox.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Eventbox.EventManagement.EventApi.Api.Controllers
{
    public abstract class OrganizerControllerBase(IOrganizationService organizationService) : ControllerBase
    {
        protected async Task EnsureMemberAsync(Guid organizationId, CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new ForbiddenApiException("Access denied.");

            if (!await organizationService.IsMemberAsync(organizationId, userId, cancellationToken))
                throw new ForbiddenApiException("You are not a member of this organization.");
        }
    }
}
