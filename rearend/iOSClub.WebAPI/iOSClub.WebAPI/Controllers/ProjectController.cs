using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iOSClub.WebAPI.Controllers;

[Authorize]
[TokenActionFilter]
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
    public async Task<ActionResult<List<ProjectModel>>> GetAllData()
    {
        var projects = await projectRepository.GetAllProjectsAsync();
        return Ok(projects);
    }

    /// <summary>
    /// 获取当前用户的项目列表
    /// </summary>
    /// <returns>用户参与的项目列表，包含任务信息</returns>
    [HttpGet("your-projects")]
    public async Task<ActionResult<List<ProjectModel>>> GetYourProjects()
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();
        if (member.Identity is "Founder" or "Member") return new List<ProjectModel>();

        var projects = await projectRepository.GetProjectsByStaffAsync(member.UserId);
        return Ok(projects);
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
    public async Task<ActionResult<ProjectModel>> CreateOrUpdateProject([FromBody] ProjectModel model)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null || string.IsNullOrEmpty(member.UserId)) return NotFound();

        var staff = await staffRepository.GetStaffByIdAsync(member.UserId);
        if (staff == null) return NotFound("用户不存在");

        var project = await projectRepository.GetProjectByIdAsync(model.Id);
        if (project == null)
        {
            // 创建新项目
            var newProject = await projectRepository.CreateProjectAsync(model, staff);
            return newProject == null ? StatusCode(500, "创建项目失败") : CreatedAtAction(nameof(GetAllData), newProject);
        }

        // 更新现有项目
        var result = await projectRepository.CreateProjectAsync(model, staff);
        return result == null ? StatusCode(500, "更新项目失败") : Ok(model);
    }

    /// <summary>
    /// 删除项目
    /// </summary>
    /// <param name="id">项目ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("delete/{id}")]
    public async Task<ActionResult> DeleteProject(string id)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null || string.IsNullOrEmpty(member.UserId)) return NotFound();

        // 检查权限
        var hasPermission = await projectRepository.HasProjectManagementPermissionAsync(member.UserId, id);
        if (!hasPermission)
            return Forbid("没有权限删除此项目");

        var result = await projectRepository.DeleteProjectAsync(id);
        if (!result)
            return NotFound("项目不存在或删除失败");

        return Ok();
    }

    /// <summary>
    /// 添加或移除项目成员（仅管理员）
    /// </summary>
    /// <param name="id">成员ID</param>
    /// <param name="projId">项目ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("change-member/{id}/{projId}")]
    [Authorize(Roles = "Founder, President, Minister")]
    public async Task<ActionResult> LetChangeProject(string id, string projId)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null || string.IsNullOrEmpty(member.UserId)) return NotFound();

        var staff = await staffRepository.GetStaffByIdAsync(member.UserId);
        if (staff == null) return NotFound("用户不存在");

        // 检查项目成员是否存在
        var projectStaffs = await projectRepository.GetProjectStaffsAsync(projId);
        var isStaffInProject = projectStaffs.Any(x => x.UserId == id);

        bool result;
        if (isStaffInProject)
        {
            result = await projectRepository.RemoveStaffFromProjectAsync(projId, id);
        }
        else
        {
            result = await projectRepository.AddStaffToProjectAsync(projId, id);
        }

        if (!result)
            return StatusCode(500, "操作失败");

        return Ok();
    }

    #endregion
}