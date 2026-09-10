namespace iOSClub.WebAPI.Common;

/// <summary>
/// 业务错误码 → HTTP 状态码的唯一映射点。
/// <para>
/// 响应体的 <c>Code</c>、<see cref="GlobalExceptionMiddleware"/> 写出的 HTTP 状态码、
/// 以及 <see cref="ApiResponseResultFilter"/> 三者都调用这里，保证它们永远一致。
/// 任何分支都不得自己挑选 HTTP 状态码。
/// </para>
/// </summary>
public static class ErrorCodeHttpMapper
{
    /// <summary>
    /// 按区间把业务错误码映射为 HTTP 状态码，个别语义与区间默认值不符的单独列出。
    /// </summary>
    public static int HttpStatusFor(int errorCode) => errorCode switch
    {
        // 显式覆盖：这些码的语义状态码与所属区间的默认值不同。
        // 3000-3999 区间默认是 403，但下面三个表达的其实是"没通过认证"。
        ErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorCode.LoginExpired => StatusCodes.Status401Unauthorized,
        ErrorCode.InvalidToken => StatusCodes.Status401Unauthorized,
        ErrorCode.InsufficientPermission => StatusCodes.Status403Forbidden,

        // 4000-4999 区间默认就是 404，这里列出是为了表意明确。
        ErrorCode.ResourceNotFound => StatusCodes.Status404NotFound,

        // 7000-7999 默认落在 400，但限流必须回 429，否则客户端读不到 Retry-After 的语义。
        ErrorCode.TooManyRequests => StatusCodes.Status429TooManyRequests,

        ErrorCode.Success => StatusCodes.Status200OK,

        // 区间默认值
        >= 1000 and < 3000 => StatusCodes.Status400BadRequest,          // 参数错误 / 业务逻辑错误
        >= 3000 and < 4000 => StatusCodes.Status403Forbidden,           // 权限错误
        >= 4000 and < 5000 => StatusCodes.Status404NotFound,            // 资源错误
        >= 5000 and < 7000 => StatusCodes.Status500InternalServerError, // 系统错误 / 外部服务错误
        >= 7000 and < 8000 => StatusCodes.Status400BadRequest,          // HTTP 相关（未覆盖的）
        _ => StatusCodes.Status400BadRequest
    };
}
