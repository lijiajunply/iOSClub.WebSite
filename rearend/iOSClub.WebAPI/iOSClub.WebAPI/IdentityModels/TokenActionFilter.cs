using System.Security.Claims;
using iOSClub.Data.ShowModels;
using Microsoft.AspNetCore.Mvc.Filters;

namespace iOSClub.WebAPI.IdentityModels;

public class TokenActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Token验证逻辑已移至GlobalAuthorizationFilter
        // 这里保留TokenActionFilter是为了向后兼容
    }
}

public static class TokenHelper
{
    public static MemberModel? GetUser(this ClaimsPrincipal? claimsPrincipal)
    {
        var claimId = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        var claimRole = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        if (claimId.IsNull() || claimRole.IsNull())
        {
            return null;
        }

        return new MemberModel()
        {
            UserId = claimId!.Value,
            Identity = claimRole!.Value,
        };
    }

    public static string? GetJwt(this HttpContext context)
    {
        var bearer = context.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(bearer) || !bearer.Contains("Bearer")) return null;
        var jwt = bearer.Split(' ');
        return jwt.Length != 2 ? null : jwt[1];
    }

    private static bool IsNull(this Claim? claim)
        => claim == null || string.IsNullOrEmpty(claim.Value);
}