using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.DataApi.Services;
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
    IJwtHelper jwtHelper)
    : ControllerBase
{
    /// <summary>
    /// 学生注册接口
    /// </summary>
    /// <param name="model">学生注册信息模型</param>
    /// <returns>成功返回JWT令牌，失败返回相应的错误信息</returns>
    [HttpPost("signup")]
    public async Task<ActionResult<string>> SignUp(StudentModel model)
    {
        var createdStudent = await studentRepository.Create(model);

        if (createdStudent == null)
        {
            return Conflict();
        }

        return jwtHelper.GetMemberToken(MemberModel.AutoCopy<StudentModel, MemberModel>(createdStudent));
    }

    /// <summary>
    /// 用户登录接口
    /// </summary>
    /// <param name="loginModel">登录信息模型，包含用户ID和用户名</param>
    /// <param name="clientId">客户端 ID</param>
    /// <param name="scope">权限</param>
    /// <returns>成功返回JWT令牌，失败返回404未找到</returns>
    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginModel loginModel, string clientId = "", string scope = "")
    {
        if (!string.IsNullOrEmpty(clientId) && string.IsNullOrEmpty(scope)) scope = "profile openid role";
        // 首先尝试使用LoginService进行学生登录
        var studentToken = await loginService.Login(loginModel, clientId, scope);
        if (!string.IsNullOrEmpty(studentToken))
        {
            return studentToken;
        }

        // 如果学生登录失败，尝试员工登录
        var staffToken = await loginService.StaffLogin(loginModel, clientId, scope);
        if (!string.IsNullOrEmpty(staffToken))
        {
            return staffToken;
        }

        return NotFound();
    }

    /// <summary>
    /// 用户登出接口
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="clientId">客户端 ID</param>
    /// <returns>成功返回true，失败返回false</returns>
    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<bool>> Logout(string userId, string clientId = "")
    {
        var result = await loginService.Logout(userId, clientId);
        return result ? Ok(true) : BadRequest(false);
    }

    /// <summary>
    /// 验证用户token是否有效
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>有效返回true，无效返回false</returns>
    [HttpGet("validate")]
    [Authorize]
    public async Task<ActionResult<bool>> ValidateToken(string userId)
    {
        var token = HttpContext.GetJwt();
        if (string.IsNullOrEmpty(token))
            return Unauthorized(false);
        var result = await loginService.ValidateToken(userId, token);
        return result ? Ok(true) : Unauthorized(false);
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
    public async Task<ActionResult<bool>> ChangePassword(string userId, string oldPassword, string newPassword)
    {
        // 验证用户身份
        var token = HttpContext.GetJwt();
        if (string.IsNullOrEmpty(token))
            return Unauthorized(false);

        var isValid = await loginService.ValidateToken(userId, token);
        if (!isValid)
            return Unauthorized(false);

        // 尝试更改密码
        var result = await loginService.ChangePassword(userId, oldPassword, newPassword);

        if (!result)
            return BadRequest("密码更新失败，可能是旧密码错误或用户不存在");

        return Ok(true);
    }

    /// <summary>
    /// 请求重置密码的验证码
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>成功返回true，失败返回相应的错误信息</returns>
    [HttpPost("request-password-reset")]
    public async Task<ActionResult<bool>> RequestPasswordReset(string userId)
    {
        var result = await loginService.RequestPasswordResetCode(userId);

        if (result) return Ok(true);
        // 检查用户是否存在
        var student = await studentRepository.GetByIdAsync(userId);
        if (student == null)
            return NotFound("用户不存在");

        // 用户存在但没有邮箱
        return BadRequest("请联系管理员进行密码更改");
    }

    /// <summary>
    /// 通过验证码重置密码
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="code">验证码</param>
    /// <param name="newPassword">新密码</param>
    /// <returns>成功返回true，失败返回相应的错误信息</returns>
    [HttpPost("reset-password")]
    public async Task<ActionResult<bool>> ResetPassword(string userId, string code, string newPassword)
    {
        var result = await loginService.ResetPasswordWithCode(userId, code, newPassword);

        if (!result)
            return BadRequest("验证码无效或密码重置失败");

        return Ok(true);
    }
}