using System.Net;
using System.Text.Json;

namespace iOSClub.WebAPI.Common;

/// <summary>
/// 全局异常处理中间件
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
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }
    
    /// <summary>
    /// 处理异常并返回标准化响应
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <param name="exception">异常对象</param>
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ApiResponse<object>
        {
            Code = (int)HttpStatusCode.InternalServerError,
            ErrorCode = ErrorCode.InternalServerError,
            Message = "服务器内部错误",
            Data = null
        };
        
        // 根据不同异常类型设置不同的错误码和消息
        switch (exception)
        {
            case ArgumentNullException:
                response.Code = (int)HttpStatusCode.BadRequest;
                response.ErrorCode = ErrorCode.ParameterEmpty;
                response.Message = "请求参数不能为空";
                break;
            case ArgumentException:
                response.Code = (int)HttpStatusCode.BadRequest;
                response.ErrorCode = ErrorCode.ParameterFormatError;
                response.Message = "请求参数格式错误";
                break;
            case InvalidOperationException:
                response.Code = (int)HttpStatusCode.BadRequest;
                response.ErrorCode = ErrorCode.InvalidStatusForOperation;
                response.Message = exception.Message;
                break;
            case KeyNotFoundException:
                response.Code = (int)HttpStatusCode.NotFound;
                response.ErrorCode = ErrorCode.ResourceNotFound;
                response.Message = "请求的资源不存在";
                break;
            default:
                // 生产环境不返回具体异常信息，避免泄露敏感信息
                if (context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
                {
                    response.Message = exception.Message;
                }
                break;
        }
        
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        
        context.Response.StatusCode = response.Code;
        await context.Response.WriteAsync(jsonResponse);
    }
}