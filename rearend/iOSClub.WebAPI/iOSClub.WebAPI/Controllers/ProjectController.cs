using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.WebAPI.Controllers;

[Authorize]
[TokenActionFilter]
[ApiController]
[Route("[controller]")]  // 使用C#推荐的API路径格式
public class ProjectController(iOSContext context, IHttpContextAccessor httpContextAccessor)
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
        return await context.Projects
            .Include(x => x.Staffs)
            .Include(x => x.Tasks)
            .ToListAsync();
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
        var staff = await context.Staffs.Include(x => x.Projects)
            .ThenInclude(x => x.Tasks)
            .FirstOrDefaultAsync(x => x.UserId == member.UserId);
        if (staff == null) return NotFound();
        return staff.Projects;
    }

    /// <summary>
    /// 获取当前用户的任务列表
    /// </summary>
    /// <returns>用户的任务列表</returns>
    [HttpGet("your-tasks")]
    public async Task<ActionResult<List<TaskModel>>> GetYourTasks()
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();
        if (member.Identity is "Founder" or "Member") return new List<TaskModel>();
        var staff = await context.Staffs.Include(x => x.Tasks)
            .FirstOrDefaultAsync(x => x.UserId == member.UserId);
        if (staff == null) return NotFound();
        return staff.Tasks;
    }

    /// <summary>
    /// 获取所有资源列表
    /// </summary>
    /// <returns>资源列表</returns>
    [HttpGet("resources")]
    public async Task<ActionResult<List<ResourceModel>>> GetResources()
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();
        if (member.Identity is "Founder" or "Member") return new List<ResourceModel>();

        return await context.Resources.ToListAsync();
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
        if (member.Identity is "Founder" or "Member") return NotFound();
        var staff = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == member.UserId);
        if (staff == null) return NotFound();
        var proj = await context.Projects.FirstOrDefaultAsync(x => x.Id == model.Id);
        if (proj == null)
        {
            model.Staffs.Add(staff);
            model.Id = model.ToHash();
            await context.Projects.AddAsync(model);
        }
        else
        {
            proj.Update(model);
        }

        await context.SaveChangesAsync();
        return model;
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
        if (member.Identity is "Member") return NotFound();

        var staff = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == member.UserId);
        if (staff == null) return NotFound();

        var project = await context.Projects.FirstOrDefaultAsync(x => x.Id == id);

        if (project == null) return NotFound();

        if (staff.Identity != "Founder" && staff.Identity != "President" &&
            !staff.Identity.Contains(project.Description)) return NoContent();

        context.Projects.Remove(project);
        await context.SaveChangesAsync();

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
        if (member.Identity is "Member") return NotFound();

        var staff = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == id);
        if (staff == null) return NotFound();

        if (!staff.Identity.Contains("Member")) return NotFound("您没有权限");

        var proj = await context.Projects.Include(x => x.Staffs).FirstOrDefaultAsync(x => x.Id == projId);
        if (proj == null) return NotFound();

        if (proj.Staffs.Any(x => x.UserId == id))
        {
            proj.Staffs.Remove(staff);
        }
        else
        {
            proj.Staffs.Add(staff);
        }

        await context.SaveChangesAsync();
        return Ok();
    }

    #endregion

    #region Task

    /// <summary>
    /// 创建或更新任务
    /// </summary>
    /// <param name="model">任务模型</param>
    /// <returns>创建或更新后的任务信息</returns>
    [HttpPost]
    public async Task<ActionResult<TaskModel>> CreateOrUpdateTask([FromBody] TaskModel model)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null || string.IsNullOrEmpty(member.UserId)) return NotFound();
        if (member.Identity is "Member") return NotFound();

        var staff = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == member.UserId);
        if (staff == null) return NotFound();

        var project = await context.Projects.FirstOrDefaultAsync(x => x.Id == model.Project.Id);

        if (project == null) return NotFound();

        if (string.IsNullOrEmpty(model.Id))
        {
            model.Id = model.ToHash();
            model.Users.Add(staff);
            project.Tasks.Add(model);
        }
        else
        {
            var task = await context.Tasks.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (task == null) return NotFound();

            task.Update(model);
        }

        await context.SaveChangesAsync();
        return model;
    }

    /// <summary>
    /// 删除任务
    /// </summary>
    /// <param name="id">任务ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("delete-task/{id}")]
    public async Task<ActionResult> DeleteTask(string id)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null || string.IsNullOrEmpty(member.UserId)) return NotFound();
        if (member.Identity is "Member") return NotFound();

        var staff = await context.Staffs.Include(x => x.Tasks).FirstOrDefaultAsync(x => x.UserId == member.UserId);
        if (staff == null) return NotFound();

        var task = staff.Tasks.FirstOrDefault(x => x.Id == id);

        if (task == null) return NotFound();

        staff.Tasks.Remove(task);
        await context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// 加入任务
    /// </summary>
    /// <param name="id">任务ID</param>
    /// <returns>加入后的任务信息</returns>
    [HttpPost("join-task/{id}")]
    public async Task<ActionResult<TaskModel>> JoinTask(string id)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null || string.IsNullOrEmpty(member.UserId)) return NotFound();
        if (member.Identity is "Member") return NotFound();

        var staff = await context.Staffs.Include(x => x.Tasks).FirstOrDefaultAsync(x => x.UserId == member.UserId);
        if (staff == null) return NotFound();

        var task = await context.Tasks.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == id);

        if (task != null) return NotFound("您已添加到项目中");

        task = await context.Tasks.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == id);
        if (task == null) return NotFound();

        task.Users.Add(staff);
        await context.SaveChangesAsync();
        return task;
    }

    /// <summary>
    /// 离开任务
    /// </summary>
    /// <param name="id">任务ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("leave-task/{id}")]
    public async Task<ActionResult> LeaveTask(string id)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null || string.IsNullOrEmpty(member.UserId)) return NotFound();
        if (member.Identity is "Member") return NotFound();

        var staff = await context.Staffs.Include(x => x.Tasks).FirstOrDefaultAsync(x => x.UserId == member.UserId);
        if (staff == null) return NotFound();

        var task = await context.Tasks.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == id);

        if (task == null) return NotFound("您未曾在这个计划中");

        task.Users.Remove(staff);
        await context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// 添加或移除任务成员（仅管理员）
    /// </summary>
    /// <param name="id">成员ID</param>
    /// <param name="taskId">任务ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("change-task-member/{id}/{taskId}")]
    [Authorize(Roles = "Founder, President, Minister")]
    public async Task<ActionResult> LetChangeTask(string id, string taskId)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null || string.IsNullOrEmpty(member.UserId)) return NotFound();
        if (member.Identity is "Member") return NotFound();

        var staff = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == id);
        if (staff == null) return NotFound();

        if (!staff.Identity.Contains("Member")) return NotFound("您没有权限");

        var task = await context.Tasks.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == taskId);
        if (task == null) return NotFound();

        if (task.Users.Any(x => x.UserId == id))
        {
            task.Users.Remove(staff);
        }
        else
        {
            task.Users.Add(staff);
        }

        await context.SaveChangesAsync();
        return Ok();
    }

    #endregion
}