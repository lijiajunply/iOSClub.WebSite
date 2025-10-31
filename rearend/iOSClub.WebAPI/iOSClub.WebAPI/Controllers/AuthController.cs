using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.DataApi.Services;
using Microsoft.AspNetCore.Mvc;
using LoginModel = iOSClub.Data.ShowModels.LoginModel;
using MemberModel = iOSClub.Data.ShowModels.MemberModel;

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
    /// <returns>成功返回JWT令牌，失败返回404未找到</returns>
    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginModel loginModel)
    {
        // 首先尝试使用LoginService进行学生登录
        var studentToken = await loginService.Login(loginModel.Id, loginModel.Name);
        if (!string.IsNullOrEmpty(studentToken))
        {
            // 学生登录成功，需要获取完整的学生信息和身份信息
            var student = await studentRepository.Get(loginModel.Id);
            
            if (student != null)
            {
                var member = MemberModel.AutoCopy<StudentModel, MemberModel>(student);
                // 对于学生用户，暂时保留默认身份
                return jwtHelper.GetMemberToken(member);
            }
        }
        
        // 如果学生登录失败，尝试员工登录
        var staffToken = await loginService.StaffLogin(loginModel.Id, loginModel.Name);
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
    /// <returns>成功返回true，失败返回false</returns>
    [HttpPost("logout")]
    public async Task<ActionResult<bool>> Logout(string userId)
    {
        var result = await loginService.Logout(userId);
        return result ? Ok(true) : BadRequest(false);
    }
    
    /// <summary>
    /// 验证用户token是否有效
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="token">要验证的token</param>
    /// <returns>有效返回true，无效返回false</returns>
    [HttpPost("validate")]
    public async Task<ActionResult<bool>> ValidateToken(string userId, string token)
    {
        var result = await loginService.ValidateToken(userId, token);
        return result ? Ok(true) : Unauthorized(false);
    }
}