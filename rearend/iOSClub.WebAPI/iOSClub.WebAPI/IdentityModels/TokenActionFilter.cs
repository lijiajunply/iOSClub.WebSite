using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using iOSClub.Data.ShowModels;
using Microsoft.AspNetCore.Mvc.Filters;

namespace iOSClub.WebAPI.IdentityModels;

public class TokenActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var bearer = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(bearer) || !bearer.Contains("Bearer")) return;

        try
        {
            var jwt = bearer.Split(' ');
            var tokenObj = new JwtSecurityToken(jwt[1]);

            var claimsIdentity = new ClaimsIdentity(tokenObj.Claims);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            context.HttpContext.User = claimsPrincipal;
        }
        catch (Exception)
        {
            // 如果JWT令牌无效，我们不设置用户主体，让默认认证处理
        }
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
            Identity = claimRole!.Value
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