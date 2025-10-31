using iOSClub.Data;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

public interface IProjectRepository
{
    Task<List<ProjectModel>> GetAllProjectsAsync();
    Task<ProjectModel?> GetProjectByIdAsync(string id);
    Task<ProjectModel?> GetProjectByTitleAsync(string title);
    Task<List<ProjectModel>> GetProjectsByDepartmentAsync(string departmentName);
    Task<List<ProjectModel>> GetProjectsByStaffAsync(string userId);
    Task<ProjectModel?> CreateProjectAsync(ProjectModel project, StaffModel creator);
    Task<bool> UpdateProjectAsync(ProjectModel project);
    Task<bool> DeleteProjectAsync(string id);
    Task<bool> ProjectExistsAsync(string id);
    Task<bool> AddStaffToProjectAsync(string projectId, string userId);
    Task<bool> RemoveStaffFromProjectAsync(string projectId, string userId);

    Task<List<StaffModel>> GetProjectStaffsAsync(string projectId);

    Task<List<TaskModel>> GetProjectTasksAsync(string projectId);

    Task<bool> HasProjectManagementPermissionAsync(string userId, string projectId);

    Task<List<ProjectModel>> GetProjectsByTimeRangeAsync(string? startTime, string? endTime);

    Task<List<ProjectModel>> SearchProjectsAsync(string searchTerm);
}

public class ProjectRepository(iOSContext context) : IProjectRepository
{
    /// <summary>
    /// 获取所有项目（包含关联数据）
    /// </summary>
    public async Task<List<ProjectModel>> GetAllProjectsAsync()
    {
        return await context.Projects
            .Include(p => p.Staffs)
            .Include(p => p.Tasks)
            .Include(p => p.Department)
            .ToListAsync();
    }

    /// <summary>
    /// 根据ID获取项目
    /// </summary>
    public async Task<ProjectModel?> GetProjectByIdAsync(string id)
    {
        return await context.Projects
            .Include(p => p.Staffs)
            .Include(p => p.Tasks)
            .Include(p => p.Department)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// 根据标题获取项目
    /// </summary>
    public async Task<ProjectModel?> GetProjectByTitleAsync(string title)
    {
        return await context.Projects
            .Include(p => p.Staffs)
            .Include(p => p.Tasks)
            .Include(p => p.Department)
            .FirstOrDefaultAsync(p => p.Title == title);
    }

    /// <summary>
    /// 获取指定部门的所有项目
    /// </summary>
    public async Task<List<ProjectModel>> GetProjectsByDepartmentAsync(string departmentName)
    {
        return await context.Projects
            .Include(p => p.Staffs)
            .Include(p => p.Tasks)
            .Include(p => p.Department)
            .Where(p => p.Department != null && p.Department.Name == departmentName)
            .ToListAsync();
    }

    /// <summary>
    /// 获取指定成员参与的所有项目
    /// </summary>
    public async Task<List<ProjectModel>> GetProjectsByStaffAsync(string userId)
    {
        return await context.Projects
            .Include(p => p.Staffs)
            .Include(p => p.Tasks)
            .Include(p => p.Department)
            .Where(p => p.Staffs.Any(s => s.UserId == userId))
            .ToListAsync();
    }

    /// <summary>
    /// 创建新项目
    /// </summary>
    public async Task<ProjectModel?> CreateProjectAsync(ProjectModel project, StaffModel creator)
    {
        try
        {
            // 设置项目ID
            if (string.IsNullOrEmpty(project.Id))
            {
                project.Id = project.ToHash();
            }

            // 添加创建者到项目成员
            project.Staffs.Add(creator);

            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();
            return project;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 更新项目信息
    /// </summary>
    public async Task<bool> UpdateProjectAsync(ProjectModel project)
    {
        try
        {
            var existingProject = await context.Projects
                .FirstOrDefaultAsync(p => p.Id == project.Id);

            if (existingProject == null)
                return false;

            existingProject.Update(project);
            context.Projects.Update(existingProject);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 删除项目
    /// </summary>
    public async Task<bool> DeleteProjectAsync(string id)
    {
        try
        {
            var project = await GetProjectByIdAsync(id);
            if (project == null)
                return false;

            context.Projects.Remove(project);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查项目是否存在
    /// </summary>
    public async Task<bool> ProjectExistsAsync(string id)
    {
        return await context.Projects.AnyAsync(p => p.Id == id);
    }

    /// <summary>
    /// 添加成员到项目
    /// </summary>
    public async Task<bool> AddStaffToProjectAsync(string projectId, string userId)
    {
        try
        {
            var project = await context.Projects
                .Include(p => p.Staffs)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            var staff = await context.Staffs
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (project == null || staff == null)
                return false;

            if (project.Staffs.Any(s => s.UserId == userId))
                return true; // 已经存在，返回成功

            project.Staffs.Add(staff);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 从项目中移除成员
    /// </summary>
    public async Task<bool> RemoveStaffFromProjectAsync(string projectId, string userId)
    {
        try
        {
            var project = await context.Projects
                .Include(p => p.Staffs)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            var staff = project?.Staffs.FirstOrDefault(s => s.UserId == userId);
            if (project == null || staff == null)
                return false;

            project.Staffs.Remove(staff);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取项目的成员列表
    /// </summary>
    public async Task<List<StaffModel>> GetProjectStaffsAsync(string projectId)
    {
        return await context.Projects
            .Where(p => p.Id == projectId)
            .SelectMany(p => p.Staffs)
            .ToListAsync();
    }

    /// <summary>
    /// 获取项目的任务列表
    /// </summary>
    public async Task<List<TaskModel>> GetProjectTasksAsync(string projectId)
    {
        return await context.Projects
            .Where(p => p.Id == projectId)
            .SelectMany(p => p.Tasks)
            .ToListAsync();
    }

    /// <summary>
    /// 检查用户是否有权限管理项目
    /// </summary>
    public async Task<bool> HasProjectManagementPermissionAsync(string userId, string projectId)
    {
        var staff = await context.Staffs
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (staff == null)
            return false;

        // Founder, President, Minister 有管理权限
        if (staff.Identity is "Founder" or "President" or "Minister")
            return true;

        // 检查是否是项目成员
        var isProjectMember = await context.Projects
            .AnyAsync(p => p.Id == projectId && p.Staffs.Any(s => s.UserId == userId));

        return isProjectMember;
    }

    /// <summary>
    /// 根据时间范围获取项目
    /// </summary>
    public async Task<List<ProjectModel>> GetProjectsByTimeRangeAsync(string? startTime, string? endTime)
    {
        var query = context.Projects
            .Include(p => p.Staffs)
            .Include(p => p.Tasks)
            .Include(p => p.Department)
            .AsQueryable();

        if (!string.IsNullOrEmpty(startTime))
        {
            query = query.Where(p => p.StartTime != null && string.Compare(p.StartTime, startTime) >= 0);
        }

        if (!string.IsNullOrEmpty(endTime))
        {
            query = query.Where(p => p.EndTime != null && string.Compare(p.EndTime, endTime) <= 0);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// 搜索项目
    /// </summary>
    public async Task<List<ProjectModel>> SearchProjectsAsync(string searchTerm)
    {
        return await context.Projects
            .Include(p => p.Staffs)
            .Include(p => p.Tasks)
            .Include(p => p.Department)
            .Where(p => p.Title.Contains(searchTerm) ||
                        p.Description.Contains(searchTerm) ||
                        (p.Department != null && p.Department.Name.Contains(searchTerm)))
            .ToListAsync();
    }
}