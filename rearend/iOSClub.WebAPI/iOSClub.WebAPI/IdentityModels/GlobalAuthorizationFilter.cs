using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using iOSClub.Data.ShowModels;
using iOSClub.DataApi.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace iOSClub.WebAPI.IdentityModels;

public class GlobalAuthorizationFilter(
    ILoginService loginService,
    IConfiguration configuration,
    ILogger<GlobalAuthorizationFilter> logger) : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var bearer = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(bearer) || !bearer.StartsWith("Bearer "))
        {
            return;
        }

        try
        {
            var jwtParts = bearer.Split(' ', 2);
            if (jwtParts.Length != 2)
            {
                logger.LogInformation("Invalid bearer token format.");
                return;
            }

            var token = jwtParts[1];

            // 验证token格式和签名
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = Environment.GetEnvironmentVariable("SECRETKEY", EnvironmentVariableTarget.Process) ??
                            configuration["Jwt:SecretKey"] ?? "";
            var key = Encoding.UTF8.GetBytes(secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "iOS Club of XAUAT",
                ValidateAudience = true,
                ValidAudience = "iOS Club of XAUAT",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(10)
            };

            // 尝试验证token
            var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // 额外验证：确保token没有过期
            var jwtToken = (JwtSecurityToken)validatedToken;
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                logger.LogInformation("Token has expired.");
                return;
            }

            // 验证token是否在Redis中存在（防止已注销的token继续使用）
            var userId = claimsPrincipal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                         claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var clientId = claimsPrincipal.FindFirst("client_id")?.Value ?? "";
            logger.LogInformation("Validating token for user {userId} and client {clientId}", userId, clientId);
            if (!string.IsNullOrEmpty(userId))
            {
                var isValid = loginService.ValidateToken(userId, token, clientId).Result;
                if (!isValid)
                {
                    logger.LogInformation("Token is not valid.");
                    return;
                }
            }

            // 设置用户上下文
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