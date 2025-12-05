using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.DataApi.Services;
using iOSClub.WebAPI.Common;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using iOSClub.Data.ShowModels;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    IStudentRepository studentRepository,
    ILoginService loginService,
    ITokenGenerator tokenGenerator,
    ILogger<AuthController> logger)
    : ControllerBase
{
    /// <summary>
    /// 学生注册接口
    /// </summary>
    /// <param name="model">学生注册信息模型</param>
    /// <returns>成功返回JWT令牌，失败返回相应的错误信息</returns>
    [HttpPost("signup")]
    public async Task<ActionResult<ApiResponse<string>>> SignUp(StudentModel model)
    {
        try
        {
            var createdStudent = await studentRepository.Create(model);

            if (createdStudent == null)
            {
                return Ok(ApiResponse<string>.Fail(ErrorCode.ResourceAlreadyExists, "用户已存在"));
            }

            var (accessToken, refreshToken) = tokenGenerator.GetMemberToken(MemberModel.AutoCopy<StudentModel, MemberModel>(createdStudent));
            // 返回访问令牌和刷新令牌，刷新令牌存储在响应头中
            Response.Headers.Append("X-Refresh-Token", refreshToken);
            return Ok(ApiResponse<string>.Success(accessToken, "注册成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "注册失败");
            }
            return Ok(ApiResponse<string>.Fail(ErrorCode.InternalServerError, "注册失败"));
        }
    }

    /// <summary>
    /// 用户登录接口
    /// </summary>
    /// <param name="loginModel">登录信息模型，包含用户ID和用户名</param>
    /// <param name="clientId">客户端 ID</param>
    /// <param name="scope">权限</param>
    /// <returns>成功返回JWT令牌，失败返回相应的错误信息</returns>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<string>>> Login(LoginModel loginModel, string clientId = "",
        string scope = "")
    {
        try
        {
            if (!string.IsNullOrEmpty(clientId) && string.IsNullOrEmpty(scope)) scope = "profile openid role";
            // 首先尝试使用LoginService进行学生登录
            var studentToken = await loginService.Login(loginModel, clientId, scope);
            if (!string.IsNullOrEmpty(studentToken))
            {
                // 从Redis中获取刷新令牌
                var refreshToken = await loginService.GetRefreshToken(loginModel.UserId, clientId);
                Response.Headers.Append("X-Refresh-Token", refreshToken);
                return Ok(ApiResponse<string>.Success(studentToken, "登录成功"));
            }

            // 如果学生登录失败，尝试员工登录
            var staffToken = await loginService.StaffLogin(loginModel, clientId, scope);
            if (!string.IsNullOrEmpty(staffToken))
            {
                // 从Redis中获取刷新令牌
                var refreshToken = await loginService.GetRefreshToken(loginModel.UserId, clientId);
                Response.Headers.Append("X-Refresh-Token", refreshToken);
                return Ok(ApiResponse<string>.Success(staffToken, "登录成功"));
            }
            
            return Ok(ApiResponse<string>.Fail(ErrorCode.UserNotFound, "用户不存在或密码错误"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "登录失败");
            }
            return Ok(ApiResponse<string>.Fail(ErrorCode.InternalServerError, "登录失败"));
        }
    }

    /// <summary>
    /// 用户登出接口
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="clientId">客户端 ID</param>
    /// <returns>成功返回true，失败返回相应的错误信息</returns>
    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<bool>>> Logout(string userId, string clientId = "")
    {
        try
        {
            var result = await loginService.Logout(userId, clientId);
            return Ok(result
                ? ApiResponse<bool>.Success(true, "登出成功")
                : ApiResponse<bool>.Fail(ErrorCode.OperationFailed, "登出失败"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "登出失败");
            }
            return Ok(ApiResponse<bool>.Fail(ErrorCode.InternalServerError, "登出失败"));
        }
    }

    /// <summary>
    /// 验证用户token是否有效
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>有效返回true，无效返回相应的错误信息</returns>
    [HttpGet("validate")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateToken(string userId)
    {
        try
        {
            var token = HttpContext.GetJwt();
            if (string.IsNullOrEmpty(token))
                return Ok(ApiResponse<bool>.Fail(ErrorCode.InvalidToken, "无效的令牌"));
            var result = await loginService.ValidateToken(userId, token);
            return Ok(result
                ? ApiResponse<bool>.Success(true, "令牌有效")
                : ApiResponse<bool>.Fail(ErrorCode.InvalidToken, "令牌无效"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "验证令牌失败");
            }
            return Ok(ApiResponse<bool>.Fail(ErrorCode.InternalServerError, "验证令牌失败"));
        }
    }

    /// <summary>
    /// 修改用户密码
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="oldPassword">旧密码</param>
    /// <param name="newPassword">新密码</param>
    /// <returns>成功返回true，失败返回相应的错误信息</returns>
    [Authorize]
    [HttpPut("change-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(string userId, string oldPassword,
        string newPassword)
    {
        try
        {
            // 验证用户身份
            var token = HttpContext.GetJwt();
            if (string.IsNullOrEmpty(token))
                return Ok(ApiResponse<bool>.Fail(ErrorCode.InvalidToken, "无效的令牌"));

            var isValid = await loginService.ValidateToken(userId, token);
            if (!isValid)
                return Ok(ApiResponse<bool>.Fail(ErrorCode.InvalidToken, "令牌无效"));

            // 尝试更改密码
            var result = await loginService.ChangePassword(userId, oldPassword, newPassword);

            return Ok(!result
                ? ApiResponse<bool>.Fail(ErrorCode.OperationFailed, "密码更新失败，可能是旧密码错误或用户不存在")
                : ApiResponse<bool>.Success(true, "密码更新成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "修改密码失败");
            }
            return Ok(ApiResponse<bool>.Fail(ErrorCode.InternalServerError, "修改密码失败"));
        }
    }

    /// <summary>
    /// 请求重置密码的验证码
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>成功返回true，失败返回相应的错误信息</returns>
    [HttpPost("request-password-reset")]
    public async Task<ActionResult<ApiResponse<bool>>> RequestPasswordReset(string userId)
    {
        try
        {
            var result = await loginService.RequestPasswordResetCode(userId);

            if (result) return Ok(ApiResponse<bool>.Success(true, "请求重置密码验证码成功"));
            // 检查用户是否存在
            var student = await studentRepository.GetByIdAsync(userId);
            return Ok(student == null
                ? ApiResponse<bool>.Fail(ErrorCode.UserNotFound, "用户不存在")
                : ApiResponse<bool>.Fail(ErrorCode.OperationFailed, "请联系管理员进行密码更改"));

            // 用户存在但没有邮箱
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "请求重置密码验证码失败");
            }
            return Ok(ApiResponse<bool>.Fail(ErrorCode.InternalServerError, "请求重置密码验证码失败"));
        }
    }

    /// <summary>
    /// 通过验证码重置密码
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="code">验证码</param>
    /// <param name="newPassword">新密码</param>
    /// <returns>成功返回true，失败返回相应的错误信息</returns>
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword(string userId, string code, string newPassword)
    {
        try
        {
            var result = await loginService.ResetPasswordWithCode(userId, code, newPassword);

            return Ok(!result
                ? ApiResponse<bool>.Fail(ErrorCode.OperationFailed, "验证码无效或密码重置失败")
                : ApiResponse<bool>.Success(true, "密码重置成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "密码重置失败");
            }
            return Ok(ApiResponse<bool>.Fail(ErrorCode.InternalServerError, "密码重置失败"));
        }
    }
    
    /// <summary>
    /// 使用刷新令牌获取新的访问令牌
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="refreshToken">刷新令牌</param>
    /// <param name="clientId">客户端ID</param>
    /// <param name="scope">权限范围</param>
    /// <returns>成功返回新的访问令牌，失败返回相应的错误信息</returns>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<string>>> RefreshToken(string userId, string refreshToken, string clientId = "", string scope = "")
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(refreshToken))
            {
                return Ok(ApiResponse<string>.Fail(ErrorCode.InvalidRequest, "用户ID和刷新令牌不能为空"));
            }
            
            var newToken = await loginService.RefreshToken(userId, refreshToken, clientId, scope);
            
            return Ok(!string.IsNullOrEmpty(newToken)
                ? ApiResponse<string>.Success(newToken, "刷新令牌成功")
                : ApiResponse<string>.Fail(ErrorCode.InvalidToken, "无效的刷新令牌"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "刷新令牌失败");
            }
            return Ok(ApiResponse<string>.Fail(ErrorCode.InternalServerError, "刷新令牌失败"));
        }
    }
}