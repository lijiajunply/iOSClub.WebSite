using iOSClub.Data;
using iOSClub.Data.ShowModels;
using iOSClub.WebAPI.Common;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class InfoController(
    IDbContextFactory<ClubContext> factory,
    IHttpContextAccessor httpContextAccessor,
    ILogger<InfoController> logger)
    : ControllerBase
{
    [HttpGet("academies")]
    public ActionResult<ApiResponse<string[]>> GetAcademies()
    {
        try
        {
            return Ok(ApiResponse<string[]>.Success(SignRecord.Academies));
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "获取学院列表失败");
            return Ok(ApiResponse<string[]>.Fail(ErrorCode.InternalServerError, "获取学院列表失败"));
        }
    }

    /// <summary>
    /// 获取当前用户可见的组织与资源信息。
    /// </summary>
    [Authorize]
    [HttpGet("user-info")]
    public async Task<ActionResult<ApiResponse<object>>> GetUserInfo()
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<object>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            await using var context = await factory.CreateDbContextAsync();
            if (member.Identity is "Member" or "Department")
                return Ok(ApiResponse<object>.Success(new { }, "获取用户信息成功"));

            if (member.Identity == "Minister")
            {
                return Ok(ApiResponse<object>.Success(new
                {
                    Resources = await context.Resources.ToArrayAsync(),
                    Departments = await context.Departments.ToArrayAsync()
                }, "获取用户信息成功"));
            }

            return Ok(ApiResponse<object>.Success(new
            {
                Total = await context.Students.CountAsync(),
                StaffsCount = await context.Staffs.CountAsync(),
                Resources = await context.Resources.ToArrayAsync(),
                Departments = await context.Departments.ToArrayAsync()
            }, "获取用户信息成功"));
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "获取用户信息失败");
            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "获取用户信息失败"));
        }
    }
}
