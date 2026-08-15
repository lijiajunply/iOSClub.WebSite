using iOSClub.Data.DataObjects;
using iOSClub.Data.DTOs;
using iOSClub.Data.VOs;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.Common;
using iOSClub.WebAPI.IdentityModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iOSClub.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ProjectController(
    IProjectRepository projectRepository,
    IStaffRepository staffRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ProjectController> logger)
    : ControllerBase
{
    #region GetValue

    [HttpGet]
    [Authorize(Roles = "Founder, President, Minister")]
    public async Task<ActionResult<ApiResponse<List<ProjectVO>>>> GetAllData()
    {
        try
        {
            var projects = await projectRepository.GetAllProjectsAsync();
            return Ok(ApiResponse<List<ProjectVO>>.Success(projects.Adapt<List<ProjectVO>>(), "获取所有项目数据成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(ex, "获取所有项目数据失败");
            return Ok(ApiResponse<List<ProjectVO>>.Fail(ErrorCode.InternalServerError, "获取所有项目数据失败"));
        }
    }

    [HttpGet("your-projects")]
    public async Task<ActionResult<ApiResponse<List<ProjectVO>>>> GetYourProjects()
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<List<ProjectVO>>.Fail(ErrorCode.Unauthorized, "用户未认证"));
            if (member.Identity is "Founder" or "Member")
                return Ok(ApiResponse<List<ProjectVO>>.Success(new List<ProjectVO>(), "获取用户项目列表成功"));
            var projects = await projectRepository.GetProjectsByStaffAsync(member.UserId);
            return Ok(ApiResponse<List<ProjectVO>>.Success(projects.Adapt<List<ProjectVO>>(), "获取用户项目列表成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(ex, "获取用户项目列表失败");
            return Ok(ApiResponse<List<ProjectVO>>.Fail(ErrorCode.InternalServerError, "获取用户项目列表失败"));
        }
    }

    #endregion

    #region Project

    [HttpPost]
    [Authorize(Roles = "Founder, President, Minister")]
    public async Task<ActionResult<ApiResponse<ProjectVO>>> CreateOrUpdateProject([FromBody] ProjectCreateUpdateDTO model)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null || string.IsNullOrEmpty(member.UserId))
                return Ok(ApiResponse<ProjectVO>.Fail(ErrorCode.Unauthorized, "用户未认证"));
            var staff = await staffRepository.GetStaffByIdAsync(member.UserId);
            if (staff == null)
                return Ok(ApiResponse<ProjectVO>.Fail(ErrorCode.UserNotFound, "用户不存在"));

            var projectDo = model.Adapt<ProjectDO>();
            var project = await projectRepository.GetProjectByIdAsync(projectDo.Id);
            if (project == null)
            {
                var newProject = await projectRepository.CreateProjectAsync(projectDo, staff);
                if (newProject == null)
                    return Ok(ApiResponse<ProjectVO>.Fail(ErrorCode.OperationFailed, "创建项目失败"));
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("创建项目成功，项目ID: {ProjectId}", newProject.Id);
                return CreatedAtAction(nameof(GetAllData), ApiResponse<ProjectVO>.Success(newProject.Adapt<ProjectVO>(), "创建项目成功"));
            }

            var updated = await projectRepository.UpdateProjectAsync(projectDo);
            if (!updated)
                return Ok(ApiResponse<ProjectVO>.Fail(ErrorCode.OperationFailed, "更新项目失败"));
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("更新项目成功，项目ID: {ProjectId}", model.Id);
            var updatedProject = await projectRepository.GetProjectByIdAsync(model.Id!);
            return Ok(ApiResponse<ProjectVO>.Success(updatedProject!.Adapt<ProjectVO>(), "更新项目成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(ex, "创建或更新项目失败，项目ID: {ProjectId}", model.Id);
            return Ok(ApiResponse<ProjectVO>.Fail(ErrorCode.InternalServerError, "创建或更新项目失败"));
        }
    }

    [HttpPost("delete/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProject(string id)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null || string.IsNullOrEmpty(member.UserId))
                return Ok(ApiResponse<object>.Fail(ErrorCode.Unauthorized, "用户未认证"));
            if (member.Identity is not ("Founder" or "President"))
                return Ok(ApiResponse<object>.Fail(ErrorCode.InsufficientPermission, "权限不足"));
            var result = await projectRepository.DeleteProjectAsync(id);
            if (!result)
                return Ok(ApiResponse<object>.Fail(ErrorCode.ResourceNotFound, "项目不存在或删除失败"));
            return Ok(ApiResponse.Success("项目删除成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(ex, "删除项目失败，项目ID: {Id}", id);
            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "删除项目失败"));
        }
    }

    #endregion

    #region Members

    [HttpPost("members/add/{projectId}/{userId}")]
    [Authorize(Roles = "Founder, President, Minister")]
    public async Task<ActionResult<ApiResponse<object>>> AddMember(string projectId, string userId)
    {
        try
        {
            var result = await projectRepository.AddStaffToProjectAsync(projectId, userId);
            if (!result)
                return Ok(ApiResponse<object>.Fail(ErrorCode.OperationFailed, "添加成员失败"));
            return Ok(ApiResponse.Success("添加成员成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(ex, "添加项目成员失败，项目ID: {ProjectId}, 用户ID: {UserId}", projectId, userId);
            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "添加成员失败"));
        }
    }

    [HttpPost("members/remove/{projectId}/{userId}")]
    [Authorize(Roles = "Founder, President, Minister")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveMember(string projectId, string userId)
    {
        try
        {
            var result = await projectRepository.RemoveStaffFromProjectAsync(projectId, userId);
            if (!result)
                return Ok(ApiResponse<object>.Fail(ErrorCode.OperationFailed, "移除成员失败"));
            return Ok(ApiResponse.Success("移除成员成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(ex, "移除项目成员失败，项目ID: {ProjectId}, 用户ID: {UserId}", projectId, userId);
            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "移除成员失败"));
        }
    }

    #endregion
}
