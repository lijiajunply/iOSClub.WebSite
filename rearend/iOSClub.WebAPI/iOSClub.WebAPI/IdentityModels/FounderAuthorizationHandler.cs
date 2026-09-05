using Microsoft.AspNetCore.Authorization;

namespace iOSClub.WebAPI.IdentityModels;

/// <summary>
/// Founder is the system superuser and satisfies every authorization requirement.
/// Authentication is still required before this handler can grant access.
/// </summary>
public sealed class FounderAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true && context.User.IsInRole("Founder"))
        {
            foreach (var requirement in context.PendingRequirements.ToList())
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
