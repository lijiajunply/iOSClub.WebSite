using System.Security.Claims;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace iOSClub.Tests.SecurityTests;

public class FounderAuthorizationHandlerTests
{
    [Fact]
    public async Task Founder_SatisfiesEveryPendingRequirement()
    {
        var requirements = new IAuthorizationRequirement[]
        {
            new RolesAuthorizationRequirement(["President"]),
            new ClaimsAuthorizationRequirement("custom-permission", ["required"])
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "founder"),
            new Claim(ClaimTypes.Role, "Founder")
        ], "Test"));
        var context = new AuthorizationHandlerContext(requirements, principal, null);

        await new FounderAuthorizationHandler().HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.Empty(context.PendingRequirements);
    }

    [Fact]
    public async Task UnauthenticatedFounder_DoesNotReceivePermissions()
    {
        var requirement = new RolesAuthorizationRequirement(["President"]);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Role, "Founder")
        ]));
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await new FounderAuthorizationHandler().HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }
}
