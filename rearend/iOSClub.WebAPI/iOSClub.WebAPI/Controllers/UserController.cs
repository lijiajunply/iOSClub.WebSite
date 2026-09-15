using Mapster;
using iOSClub.Data.DataObjects;
using iOSClub.Data.DTOs;
using iOSClub.Data.VOs;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.Common;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(
    IStudentRepository studentRepository,
    IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    [Authorize]
    [HttpGet("data")]
    public async Task<ActionResult<ApiResponse<MemberVO>>> GetData()
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null)
            return Ok(ApiResponse<MemberVO>.Fail(ErrorCode.Unauthorized, "用户未认证"));

        // Founder 也走这里。曾经这里对 Founder 直接返回 GetUser() 的存根，
        // 但那个 MemberVO 只有 UserId + Identity，于是 Founder 在概览页看不到姓名、
        // 个人档案页整张表单是空的。
        var student = await studentRepository.GetByIdAsync(member.UserId);
        if (student == null)
            return Ok(ApiResponse<MemberVO>.Fail(ErrorCode.UserNotFound, "用户不存在"));

        var result = student.Adapt<MemberVO>();
        result.Identity = member.Identity;
        return Ok(ApiResponse<MemberVO>.Success(result, "获取用户信息成功"));
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateProfile([FromBody] StudentUpdateDTO dto)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null || member.UserId != dto.UserId)
            return Ok(ApiResponse<object>.Fail(ErrorCode.InsufficientPermission, "权限不足"));

        // 走 UpdateProfileAsync 而不是通用更新：这个接口是用户改自己的资料，
        // 不该有任何路径能碰到 PasswordHash（改密码在 Auth/change-password）。
        var result = await studentRepository.UpdateProfileAsync(dto.Adapt<StudentDO>());
        return result
            ? Ok(ApiResponse.Success("更新用户资料成功"))
            : Ok(ApiResponse<object>.Fail(ErrorCode.OperationFailed, "更新用户资料失败"));
    }
}
