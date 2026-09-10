using System.Text.Json;
using iOSClub.DataApi.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace iOSClub.WebAPI.Common;

/// <summary>
/// 全局异常处理中间件 —— 本项目的唯一错误出口。
/// <para>
/// 契约：body 恒为 <see cref="ApiResponse{T}"/>，HTTP 状态码恒等于 body 的 <c>code</c>，
/// <c>errorCode == 0</c> 表示成功。状态码由 <see cref="ErrorCodeHttpMapper"/> 从错误码推导，
/// 本类不允许自己挑 HTTP 状态码。
/// </para>
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="next">下一个中间件</param>
    /// <param name="logger">日志记录器</param>
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// 中间件执行方法
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // 生成或获取请求ID
        var requestId = context.TraceIdentifier;

        try
        {
            // 将请求ID添加到响应头，方便客户端追踪
            context.Response.Headers.Append("X-Request-ID", requestId);
            await _next(context);
        }
        catch (Exception ex)
        {
            // 记录请求上下文信息
            var requestInfo = new
            {
                // 具体的 action 名称，便于定位是哪个接口抛的
                Endpoint = context.GetEndpoint() is { } endpoint ? endpoint.DisplayName : null,
                context.Request.Method,
                context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                RemoteIp = context.Connection.RemoteIpAddress?.ToString(),
                RequestId = requestId,
                CorrelationId = context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId)
                    ? correlationId.ToString()
                    : "N/A",
                context.Request.ContentType,
                context.Request.ContentLength
            };

            // 响应已经开始写入时状态码改不了，硬改会抛 InvalidOperationException 把原始异常盖掉。
            // 这种情况只能记录后交给 Kestrel 中断连接。
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex, "响应已开始写入，无法改写响应。Request: {@RequestInfo}", requestInfo);
                throw;
            }

            var response = BuildResponse(context, ex, requestId);

            // 按映射出的 HTTP 状态码分级：客户端原因导致的失败（4xx）不该刷 Error 级日志，
            // 否则 76 处 catch 被移除后，正常的参数校验失败会淹没真正的服务端故障。
            var logLevel = response.Code >= StatusCodes.Status500InternalServerError
                ? LogLevel.Error
                : LogLevel.Warning;
            _logger.Log(logLevel, ex, "请求处理失败。Request: {@RequestInfo}", requestInfo);

            await WriteResponseAsync(context, response);
        }
    }

    /// <summary>
    /// 把异常翻译成统一响应模型。纯函数，不写响应。
    /// </summary>
    private static ApiResponse<object> BuildResponse(HttpContext context, Exception exception, string requestId)
    {
        var env = context.RequestServices.GetRequiredService<IHostEnvironment>();

        var response = new ApiResponse<object>
        {
            ErrorCode = ErrorCode.InternalServerError,
            Message = "服务器内部错误",
            Data = null
        };

        switch (exception)
        {
            // 自定义异常一律以异常自身携带的错误码为准。
            // 之前这几个分支把调用方传进来的 errorCode 丢弃、改成硬编码，
            // 导致 BusinessException(ResourceAlreadyExists) 报出来的却是"操作失败"。
            // 注意顺序：ValidationException 必须排在 CustomException 之前（它是子类）。
            case iOSClub.DataApi.Exceptions.ValidationException validationException:
                response.ErrorCode = validationException.ErrorCode;
                response.Message = validationException.Message;
                response.Detail = validationException.ValidationErrors is { Count: > 0 } errors
                    ? string.Join(", ", errors.SelectMany(e => e.Value.Select(error => $"{e.Key}: {error}")))
                    : validationException.Detail;
                break;

            case CustomException customException:
                response.ErrorCode = customException.ErrorCode;
                response.Message = customException.Message;
                response.Detail = customException.Detail;
                break;

            case ArgumentNullException argNullException:
                response.ErrorCode = ErrorCode.ParameterEmpty;
                response.Message = $"请求参数不能为空: {argNullException.ParamName}";
                break;

            case ArgumentException argException:
                response.ErrorCode = ErrorCode.ParameterFormatError;
                response.Message = string.IsNullOrEmpty(argException.ParamName)
                    ? argException.Message
                    : $"请求参数格式错误: {argException.ParamName} - {argException.Message}";
                break;

            case InvalidOperationException invalidOpException:
                response.ErrorCode = ErrorCode.InvalidStatusForOperation;
                response.Message = invalidOpException.Message;
                break;

            case KeyNotFoundException:
                response.ErrorCode = ErrorCode.ResourceNotFound;
                response.Message = "请求的资源不存在";
                break;

            case FileNotFoundException:
                response.ErrorCode = ErrorCode.FileNotFound;
                response.Message = "请求的文件不存在";
                break;

            case JsonException jsonException:
                response.ErrorCode = ErrorCode.ParameterFormatError;
                response.Message = "请求数据格式错误";
                if (env.IsDevelopment())
                {
                    response.Detail = jsonException.Message;
                }

                break;

            case UnauthorizedAccessException:
                response.ErrorCode = ErrorCode.Unauthorized;
                response.Message = "未授权访问";
                break;

            case System.Security.Authentication.AuthenticationException authException:
                response.ErrorCode = ErrorCode.InvalidToken;
                response.Message = authException.Message;
                break;

            case SecurityTokenExpiredException:
                response.ErrorCode = ErrorCode.LoginExpired;
                response.Message = "访问令牌已过期";
                break;

            // 处理验证异常
            case FluentValidation.ValidationException fluentValidationException:
                response.ErrorCode = ErrorCode.ParameterValidationFailed;
                response.Message = "请求参数验证失败";
                response.Detail = string.Join(", ",
                    fluentValidationException.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                break;

            // 处理数据库异常
            case Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException:
                response.ErrorCode = ErrorCode.DataProcessingFailed;
                response.Message = "数据并发冲突，同一资源被同时修改";
                break;

            case Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateEx:
                response.ErrorCode = ErrorCode.DataProcessingFailed;
                response.Message = "数据更新失败";
                if (env.IsDevelopment())
                {
                    response.Detail = dbUpdateEx.Message;
                }

                break;

            case HttpRequestException httpEx:
                response.ErrorCode = ErrorCode.NetworkError;
                response.Message = "网络请求失败";
                if (env.IsDevelopment())
                {
                    response.Detail = httpEx.Message;
                }

                break;

            case TimeoutException timeoutEx:
                response.ErrorCode = ErrorCode.ExternalServiceTimeout;
                response.Message = "操作超时";
                if (env.IsDevelopment())
                {
                    response.Detail = timeoutEx.Message;
                }

                break;

            case OperationCanceledException canceledEx:
                response.ErrorCode = ErrorCode.OperationFailed;
                response.Message = "操作被取消";
                if (env.IsDevelopment())
                {
                    response.Detail = canceledEx.Message;
                }

                break;

            default:
                // 生产环境不返回具体异常信息，避免泄露敏感信息
                if (env.IsDevelopment())
                {
                    response.Message = exception.Message;
                    response.Detail = exception.StackTrace;
                }

                break;
        }

        // HTTP 状态码统一从错误码推导，任何分支都不得自行指定
        response.Code = ErrorCodeHttpMapper.HttpStatusFor(response.ErrorCode);
        response.RequestId = requestId;
        response.Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        return response;
    }

    /// <summary>
    /// 写出统一响应。
    /// </summary>
    private static async Task WriteResponseAsync(HttpContext context, ApiResponse<object> response)
    {
        context.Response.ContentType = "application/json";

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        context.Response.StatusCode = response.Code;
        await context.Response.WriteAsync(jsonResponse);
    }
}
