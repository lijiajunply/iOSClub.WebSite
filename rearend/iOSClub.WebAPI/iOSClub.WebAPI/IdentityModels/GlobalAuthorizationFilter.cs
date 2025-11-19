using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        // 如果用户已经通过其他方式认证，则不处理
        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            logger.LogInformation("已认证成功");
            return;
        }

        var bearer = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();

        // 如果是SSO userinfo端点，也检查access_token查询参数
        var accessToken = context.HttpContext.Request.Query["access_token"].FirstOrDefault();

        // 如果既没有Authorization头也没有access_token查询参数，则返回
        if (string.IsNullOrEmpty(bearer) && string.IsNullOrEmpty(accessToken))
        {
            logger.LogInformation("无 Token");
            return;
        }

        // 优先使用Authorization头中的Bearer token，如果没有则使用查询参数中的access_token
        var token = !string.IsNullOrEmpty(bearer) && bearer.StartsWith("Bearer ")
            ? bearer["Bearer ".Length..]
            : accessToken;

        if (string.IsNullOrEmpty(token))
        {
            logger.LogInformation("无 JWT");
            return;
        }

        try
        {
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
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            // 尝试验证token
            var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // 额外验证：确保token没有过期
            var jwtToken = (JwtSecurityToken)validatedToken;
            if (jwtToken.ValidTo < DateTime.UtcNow) return;

            // 验证token是否在Redis中存在（防止已注销的token继续使用）
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var clientId = claimsPrincipal.FindFirst("client_id")?.Value ?? "";
            if (!string.IsNullOrEmpty(userId))
            {
                logger.LogInformation("Validating token for user {UserId} and client {ClientId}", userId, clientId);
                var isValid = loginService.ValidateToken(userId, token, clientId).Result;
                if (!isValid) return;
            }

            // 设置用户上下文
            context.HttpContext.User = claimsPrincipal;
        }
        catch (Exception ex)
        {
            // 记录异常但不中断流程
            logger.LogError("Token validation error: {Message}", ex.Message);
        }
    }
}