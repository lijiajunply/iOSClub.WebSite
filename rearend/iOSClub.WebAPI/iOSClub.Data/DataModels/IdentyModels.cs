using System.Security.Claims;

namespace iOSClub.Data.DataModels;

public static class ClaimsPrincipalExtensions
{
    public static StaffModel? GetStaff(this ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
            return null;

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var name = principal.FindFirst(ClaimTypes.Name)?.Value;
        var identity = principal.FindFirst("Identity")?.Value ?? "Member";

        if (string.IsNullOrEmpty(userId))
            return null;

        return new StaffModel
        {
            UserId = userId,
            Name = name ?? "",
            Identity = identity
        };
    }
}