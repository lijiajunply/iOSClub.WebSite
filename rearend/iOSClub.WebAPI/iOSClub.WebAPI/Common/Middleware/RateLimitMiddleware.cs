using System.Threading.RateLimiting;

namespace iOSClub.WebAPI.Common.Middleware;

/// <summary>
/// 请求频率限制中间件，用于防止暴力破解
/// </summary>
public class RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
{
    // 限制每个IP每分钟最多100个请求
    private readonly TokenBucketRateLimiter _limiter = new(new TokenBucketRateLimiterOptions
    {
        TokenLimit = 100,
        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
        TokensPerPeriod = 100,
        AutoReplenishment = true
    });

    public async Task InvokeAsync(HttpContext context)
    {
        // 获取客户端IP地址
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // 创建速率限制键
        var rateLimitKey = $"rate_limit:{clientIp}";

        // 尝试获取令牌
        var lease = await _limiter.AcquireAsync(1, CancellationToken.None);

        if (lease.IsAcquired)
        {
            // 如果获取到令牌，继续处理请求
            await next(context);
        }
        else
        {
            // 如果没有获取到令牌，返回429 Too Many Requests
            logger.LogWarning("请求频率限制触发，客户端IP：{ClientIp}", clientIp);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.Append("Retry-After", "60");
            await context.Response.WriteAsJsonAsync(ApiResponse<string>.Fail(ErrorCode.TooManyRequests,
                "请求频率过高，请稍后再试"));
        }
    }
}