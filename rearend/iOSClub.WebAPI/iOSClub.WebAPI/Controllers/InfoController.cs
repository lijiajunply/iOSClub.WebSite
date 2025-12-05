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
    /// <summary>
    /// 获取所有学院列表
    /// </summary>
    /// <returns>学院名称字符串数组</returns>
    [HttpGet("academies")]
    public ActionResult<ApiResponse<string[]>> GetAcademies()
    {
        try
        {
            return Ok(ApiResponse<string[]>.Success(SignRecord.Academies));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "获取学院列表失败");
            }

            return Ok(ApiResponse<string[]>.Fail(ErrorCode.InternalServerError, "获取学院列表失败"));
        }
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <remarks>
    /// 根据用户身份返回不同级别的信息：
    /// - 普通成员：返回空
    /// - 部门成员：返回任务和项目信息
    /// - 部长：返回任务、项目和资源信息
    /// - 社长/创始人：返回所有系统信息
    /// </remarks>
    /// <returns>根据用户身份返回相应的用户信息</returns>
    [Authorize]
    [HttpGet("user-info")]
    public async Task<ActionResult<ApiResponse<object>>> GetUserInfo()
    {
        try
        {
            await using var context = await factory.CreateDbContextAsync();

            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<object>.Fail(ErrorCode.Unauthorized, "用户未认证"));
            var student = await context.Students.FirstOrDefaultAsync(x => x.UserId == member.UserId);
            if (student == null)
                return Ok(ApiResponse<object>.Fail(ErrorCode.UserNotFound, "用户不存在"));

            if (member.Identity == "Member")
            {
                return Ok(ApiResponse.Success("获取用户信息成功"));
            }

            if (member.Identity == "Department")
            {
                var data = new
                {
                    Tasks = await context.Tasks.Where(x => x.Users.Any(y => y.UserId == member.UserId))
                        .ToArrayAsync(),
                    Projects = await context.Projects.Where(x => x.Staffs.Any(y => y.UserId == member.UserId))
                        .ToArrayAsync(),
                };
                return Ok(ApiResponse<object>.Success(data, "获取用户信息成功"));
            }

            if (member.Identity == "Minister")
            {
                var staff = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == member.UserId);

                if (staff == null)
                    return Ok(ApiResponse<object>.Fail(ErrorCode.UserNotFound, "用户不存在"));

                var data = new
                {
                    Tasks = await context.Tasks.Where(x => x.Users.Any(y => y.UserId == member.UserId))
                        .ToArrayAsync(),
                    Projects = await context.Projects
                        .Where(x => x.Staffs.Any(y => y.UserId == member.UserId) || x.Department == staff.Department)
                        .ToArrayAsync(),
                    Resources = await context.Resources.ToArrayAsync(),
                };
                return Ok(ApiResponse<object>.Success(data, "获取用户信息成功"));
            }

            var adminData = new
            {
                Total = await context.Students.CountAsync(),
                StaffsCount = await context.Staffs.CountAsync(),
                Projects = await context.Projects.ToArrayAsync(),
                Tasks = await context.Tasks.ToArrayAsync(),
                Resources = await context.Resources.ToArrayAsync(),
                Departments = await context.Departments.ToArrayAsync()
            };
            return Ok(ApiResponse<object>.Success(adminData, "获取用户信息成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "获取用户信息失败");
            }

            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "获取用户信息失败"));
        }
    }
}