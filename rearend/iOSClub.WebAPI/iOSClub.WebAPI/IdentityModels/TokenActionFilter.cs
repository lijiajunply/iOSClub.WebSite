using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using iOSClub.Data.ShowModels;
using Microsoft.AspNetCore.Mvc.Filters;

namespace iOSClub.WebAPI.IdentityModels;

public class TokenActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // 如果用户已经通过其他方式认证，则不处理
        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            return;

        var bearer = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(bearer) || !bearer.StartsWith("Bearer ")) 
            return;

        try
        {
            var jwtParts = bearer.Split(' ', 2);
            if (jwtParts.Length != 2) 
                return;

            var token = jwtParts[1];
            var tokenObj = new JwtSecurityToken(token);

            // 确保token没有过期
            if (tokenObj.ValidTo < DateTime.UtcNow)
                return;

            // 创建带有认证类型的ClaimsIdentity，使用JWT默认认证方案
            var claimsIdentity = new ClaimsIdentity(tokenObj.Claims, Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            context.HttpContext.User = claimsPrincipal;
        }
        catch (Exception ex)
        {
            // 记录异常但不中断流程
            Console.WriteLine($"Token validation error: {ex.Message}");
        }
    }
}

public static class TokenHelper
{
    public static MemberModel? GetUser(this ClaimsPrincipal? claimsPrincipal)
    {
        var claimId = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        var claimRole = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        var name = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName);
        var email = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        if (claimId.IsNull() || claimRole.IsNull() || name.IsNull() || email.IsNull())
        {
            return null;
        }

        return new MemberModel()
        {
            UserId = claimId!.Value,
            Identity = claimRole!.Value,
            UserName = name!.Value,
            EMail = email!.Value
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