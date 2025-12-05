using System.Net;
using System.Text.Json;
using FluentValidation;
using iOSClub.WebAPI.Common.Exceptions;

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
            // 记录异常日志，包含请求上下文信息
            _logger.LogError(ex, "Unhandled exception occurred. Request: {Method} {Path}, UserAgent: {UserAgent}, RemoteIp: {RemoteIp}",
                context.Request.Method,
                context.Request.Path,
                context.Request.Headers.UserAgent.ToString(),
                context.Connection.RemoteIpAddress?.ToString());
            
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
        var env = context.RequestServices.GetRequiredService<IHostEnvironment>();
        
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
            // 处理自定义异常
            case CustomException customException:
                response.Code = customException.HttpStatusCode;
                response.ErrorCode = customException.ErrorCode;
                response.Message = customException.Message;
                response.Detail = customException.Detail;
                break;
            
            // 处理参数异常
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
            
            // 处理业务逻辑异常
            case InvalidOperationException:
                response.Code = (int)HttpStatusCode.BadRequest;
                response.ErrorCode = ErrorCode.InvalidStatusForOperation;
                response.Message = exception.Message;
                break;
            
            // 处理资源访问异常
            case KeyNotFoundException:
                response.Code = (int)HttpStatusCode.NotFound;
                response.ErrorCode = ErrorCode.ResourceNotFound;
                response.Message = "请求的资源不存在";
                break;
            
            // 处理认证授权异常
            case UnauthorizedAccessException:
                response.Code = (int)HttpStatusCode.Unauthorized;
                response.ErrorCode = ErrorCode.Unauthorized;
                response.Message = "未授权访问";
                break;
            
            case System.Security.Authentication.AuthenticationException:
                response.Code = (int)HttpStatusCode.Unauthorized;
                response.ErrorCode = ErrorCode.InvalidToken;
                response.Message = "认证失败";
                break;
            
            // 处理验证异常
            case FluentValidation.ValidationException validationException:
                response.Code = (int)HttpStatusCode.BadRequest;
                response.ErrorCode = ErrorCode.ParameterValidationFailed;
                response.Message = "请求参数验证失败";
                response.Detail = string.Join(", ", validationException.Errors.Select(e => e.ErrorMessage));
                break;
            
            // 处理数据库异常
            case Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException:
                response.Code = (int)HttpStatusCode.Conflict;
                response.ErrorCode = ErrorCode.DataProcessingFailed;
                response.Message = "数据并发冲突";
                break;
            
            case Microsoft.EntityFrameworkCore.DbUpdateException:
                response.Code = (int)HttpStatusCode.BadRequest;
                response.ErrorCode = ErrorCode.DataProcessingFailed;
                response.Message = "数据更新失败";
                break;
            
            // 处理其他异常
            default:
                // 生产环境不返回具体异常信息，避免泄露敏感信息
                if (env.IsDevelopment())
                {
                    response.Message = exception.Message;
                    response.Detail = exception.StackTrace;
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