using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.Common;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iOSClub.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")] // 使用C#推荐的API路径格式
public class ProjectController(
    IProjectRepository projectRepository,
    IStaffRepository staffRepository,
    IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    #region GetValue

    /// <summary>
    /// 获取所有项目数据（仅管理员）
    /// </summary>
    /// <returns>项目列表，包含关联的员工和任务信息</returns>
    [HttpGet]
    [Authorize(Roles = "Founder, President, Minister")]
    public async Task<ActionResult<ApiResponse<List<ProjectModel>>>> GetAllData()
    {
        try
        {
            var projects = await projectRepository.GetAllProjectsAsync();
            return Ok(ApiResponse<List<ProjectModel>>.Success(projects, "获取所有项目数据成功"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<List<ProjectModel>>.Fail(ErrorCode.InternalServerError, "获取所有项目数据失败"));
        }
    }

    /// <summary>
    /// 获取当前用户的项目列表
    /// </summary>
    /// <returns>用户参与的项目列表，包含任务信息</returns>
    [HttpGet("your-projects")]
    public async Task<ActionResult<ApiResponse<List<ProjectModel>>>> GetYourProjects()
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null) 
                return Ok(ApiResponse<List<ProjectModel>>.Fail(ErrorCode.Unauthorized, "用户未认证"));
            
            if (member.Identity is "Founder" or "Member") 
                return Ok(ApiResponse<List<ProjectModel>>.Success(new List<ProjectModel>(), "获取用户项目列表成功"));

            var projects = await projectRepository.GetProjectsByStaffAsync(member.UserId);
            return Ok(ApiResponse<List<ProjectModel>>.Success(projects, "获取用户项目列表成功"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<List<ProjectModel>>.Fail(ErrorCode.InternalServerError, "获取用户项目列表失败"));
        }
    }

    #endregion

    #region Project

    /// <summary>
    /// 创建或更新项目（仅管理员）
    /// </summary>
    /// <param name="model">项目模型</param>
    /// <returns>创建或更新后的项目信息</returns>
    [HttpPost]
    [Authorize(Roles = "Founder, President, Minister")]
    public async Task<ActionResult<ApiResponse<ProjectModel>>> CreateOrUpdateProject([FromBody] ProjectModel model)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null || string.IsNullOrEmpty(member.UserId)) 
                return Ok(ApiResponse<ProjectModel>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            var staff = await staffRepository.GetStaffByIdAsync(member.UserId);
            if (staff == null) 
                return Ok(ApiResponse<ProjectModel>.Fail(ErrorCode.UserNotFound, "用户不存在"));

            var project = await projectRepository.GetProjectByIdAsync(model.Id);
            if (project == null)
            {
                // 创建新项目
                var newProject = await projectRepository.CreateProjectAsync(model, staff);
                if (newProject == null) 
                    return Ok(ApiResponse<ProjectModel>.Fail(ErrorCode.OperationFailed, "创建项目失败"));
                    
                return CreatedAtAction(nameof(GetAllData), ApiResponse<ProjectModel>.Success(newProject, "创建项目成功"));
            }

            // 更新现有项目
            var result = await projectRepository.CreateProjectAsync(model, staff);
            if (result == null) 
                return Ok(ApiResponse<ProjectModel>.Fail(ErrorCode.OperationFailed, "更新项目失败"));
                
            return Ok(ApiResponse<ProjectModel>.Success(model, "更新项目成功"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<ProjectModel>.Fail(ErrorCode.InternalServerError, "创建或更新项目失败"));
        }
    }

    /// <summary>
    /// 删除项目
    /// </summary>
    /// <param name="id">项目ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("delete/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProject(string id)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null || string.IsNullOrEmpty(member.UserId)) 
                return Ok(ApiResponse<object>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            // 检查权限
            var hasPermission = await projectRepository.HasProjectManagementPermissionAsync(member.UserId, id);
            if (!hasPermission)
                return Ok(ApiResponse<object>.Fail(ErrorCode.InsufficientPermission, "没有权限删除此项目"));

            var result = await projectRepository.DeleteProjectAsync(id);
            if (!result)
                return Ok(ApiResponse<object>.Fail(ErrorCode.ResourceNotFound, "项目不存在或删除失败"));

            return Ok(ApiResponse<object>.Success(null, "删除项目成功"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "删除项目失败"));
        }
    }

    /// <summary>
    /// 添加或移除项目成员（仅管理员）
    /// </summary>
    /// <param name="id">成员ID</param>
    /// <param name="projId">项目ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("change-member/{id}/{projId}")]
    [Authorize(Roles = "Founder, President, Minister")]
    public async Task<ActionResult<ApiResponse<object>>> LetChangeProject(string id, string projId)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null || string.IsNullOrEmpty(member.UserId)) 
                return Ok(ApiResponse<object>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            var staff = await staffRepository.GetStaffByIdAsync(member.UserId);
            if (staff == null) 
                return Ok(ApiResponse<object>.Fail(ErrorCode.UserNotFound, "用户不存在"));

            // 检查项目成员是否存在
            var projectStaffs = await projectRepository.GetProjectStaffsAsync(projId);
            var isStaffInProject = projectStaffs.Any(x => x.UserId == id);

            bool result;
            string actionMessage;
            if (isStaffInProject)
            {
                result = await projectRepository.RemoveStaffFromProjectAsync(projId, id);
                actionMessage = "移除项目成员成功";
            }
            else
            {
                result = await projectRepository.AddStaffToProjectAsync(projId, id);
                actionMessage = "添加项目成员成功";
            }

            if (!result)
                return Ok(ApiResponse<object>.Fail(ErrorCode.OperationFailed, "操作失败"));

            return Ok(ApiResponse<object>.Success(null, actionMessage));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "操作失败"));
        }
    }

    #endregion
}