using iOSClub.Data;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

/// <summary>
/// 项目仓库接口，提供项目数据的CRUD操作和查询功能
/// </summary>
public interface IProjectRepository
{
    /// <summary>
    /// 获取所有项目
    /// </summary>
    /// <returns>项目列表</returns>
    Task<List<ProjectModel>> GetAllProjectsAsync();
    
    /// <summary>
    /// 根据ID获取项目
    /// </summary>
    /// <param name="id">项目ID</param>
    /// <returns>项目模型，如果找不到则返回null</returns>
    Task<ProjectModel?> GetProjectByIdAsync(string id);
    
    /// <summary>
    /// 根据标题获取项目
    /// </summary>
    /// <param name="title">项目标题</param>
    /// <returns>项目模型，如果找不到则返回null</returns>
    Task<ProjectModel?> GetProjectByTitleAsync(string title);
    
    /// <summary>
    /// 根据部门获取项目
    /// </summary>
    /// <param name="departmentName">部门名称</param>
    /// <returns>项目列表</returns>
    Task<List<ProjectModel>> GetProjectsByDepartmentAsync(string departmentName);
    
    /// <summary>
    /// 根据成员获取项目
    /// </summary>
    /// <param name="userId">成员ID</param>
    /// <returns>项目列表</returns>
    Task<List<ProjectModel>> GetProjectsByStaffAsync(string userId);
    
    /// <summary>
    /// 创建项目
    /// </summary>
    /// <param name="project">项目模型</param>
    /// <param name="creator">创建者</param>
    /// <returns>创建的项目模型，如果创建失败则返回null</returns>
    Task<ProjectModel?> CreateProjectAsync(ProjectModel project, StaffModel creator);
    
    /// <summary>
    /// 更新项目
    /// </summary>
    /// <param name="project">项目模型</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateProjectAsync(ProjectModel project);
    
    /// <summary>
    /// 删除项目
    /// </summary>
    /// <param name="id">项目ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteProjectAsync(string id);
    
    /// <summary>
    /// 检查项目是否存在
    /// </summary>
    /// <param name="id">项目ID</param>
    /// <returns>项目是否存在</returns>
    Task<bool> ProjectExistsAsync(string id);
    
    /// <summary>
    /// 添加成员到项目
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <param name="userId">成员ID</param>
    /// <returns>是否添加成功</returns>
    Task<bool> AddStaffToProjectAsync(string projectId, string userId);
    
    /// <summary>
    /// 从项目中移除成员
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <param name="userId">成员ID</param>
    /// <returns>是否移除成功</returns>
    Task<bool> RemoveStaffFromProjectAsync(string projectId, string userId);
    
    /// <summary>
    /// 获取项目成员列表
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <returns>成员列表</returns>
    Task<List<StaffModel>> GetProjectStaffsAsync(string projectId);
    
    /// <summary>
    /// 获取项目任务列表
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <returns>任务列表</returns>
    Task<List<TaskModel>> GetProjectTasksAsync(string projectId);
    
    /// <summary>
    /// 检查用户是否有项目管理权限
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="projectId">项目ID</param>
    /// <returns>是否有权限</returns>
    Task<bool> HasProjectManagementPermissionAsync(string userId, string projectId);
    
    /// <summary>
    /// 根据时间范围获取项目
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>项目列表</returns>
    Task<List<ProjectModel>> GetProjectsByTimeRangeAsync(string? startTime, string? endTime);
    
    /// <summary>
    /// 搜索项目
    /// </summary>
    /// <param name="searchTerm">搜索词</param>
    /// <returns>项目列表</returns>
    Task<List<ProjectModel>> SearchProjectsAsync(string searchTerm);
}

public class ProjectRepository(IDbContextFactory<ClubContext> factory) : IProjectRepository
{
    /// <summary>
    /// 获取所有项目（包含关联数据）
    /// </summary>
    public async Task<List<ProjectModel>> GetAllProjectsAsync()
    {
        await using var context = await factory.CreateDbContextAsync();
        return (await context.Projects
            .Include(p => p.Staffs)
            .Include(p => p.Tasks)
            .Include(p => p.Department)
            .ToListAsync())
            .Select(x => x.OutputWhenOtherList())
            .ToList();
    }

    /// <summary>
    /// 根据ID获取项目
    /// </summary>
    public async Task<ProjectModel?> GetProjectByIdAsync(string id)
    {
        await using var context = await factory.CreateDbContextAsync();
        var project = await context.Projects
            .Include(p => p.Staffs)
            .Include(p => p.Tasks)
            .Include(p => p.Department)
            .FirstOrDefaultAsync(p => p.Id == id);
        return project?.OutputWhenOtherList();
    }

    /// <summary>
    /// 根据标题获取项目
    /// </summary>
    public async Task<ProjectModel?> GetProjectByTitleAsync(string title)
    {
        await using var context = await factory.CreateDbContextAsync();
        var project = await context.Projects
            .Include(p => p.Staffs)
            .Include(p => p.Tasks)
            .Include(p => p.Department)
            .FirstOrDefaultAsync(p => p.Title == title);
        return project?.OutputWhenOtherList();
    }

    /// <summary>
    /// 获取指定部门的所有项目
    /// </summary>
    public async Task<List<ProjectModel>> GetProjectsByDepartmentAsync(string departmentName)
    {
        await using var context = await factory.CreateDbContextAsync();
        return (await context.Projects
            .Include(p => p.Staffs)
            .Include(p => p.Tasks)
            .Include(p => p.Department)
            .Where(p => p.Department != null && p.Department.Name == departmentName)
            .ToListAsync())
            .Select(x => x.OutputWhenOtherList())
            .ToList();
    }

    /// <summary>
    /// 获取指定成员参与的所有项目
    /// </summary>
    public async Task<List<ProjectModel>> GetProjectsByStaffAsync(string userId)
    {
        await using var context = await factory.CreateDbContextAsync();
        return (await context.Projects
            .Include(p => p.Staffs)
            .Include(p => p.Tasks)
            .Include(p => p.Department)
            .Where(p => p.Staffs.Any(s => s.UserId == userId))
            .ToListAsync())
            .Select(x => x.OutputWhenOtherList())
            .ToList();
    }

    /// <summary>
    /// 创建新项目
    /// </summary>
    public async Task<ProjectModel?> CreateProjectAsync(ProjectModel project, StaffModel creator)
    {
        await using var context = await factory.CreateDbContextAsync();
        try
        {
            // 设置项目ID
            if (string.IsNullOrEmpty(project.Id))
            {
                project.Id = project.GetHashKey();
            }

            // 添加创建者到项目成员
            // 首先检查创建者是否已经在数据库中存在
            var existingStaff = await context.Staffs.FindAsync(creator.UserId);
            if (existingStaff != null)
            {
                // 如果存在，使用数据库中的 StaffModel
                project.Staffs.Add(existingStaff);
            }
            else
            {
                // 如果不存在，添加新的 StaffModel
                project.Staffs.Add(creator);
            }

            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();
            return project;
        }
        catch (Exception ex)
        {
            // 记录异常，便于调试
            Console.WriteLine($"Error creating project: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// 更新项目信息
    /// </summary>
    public async Task<bool> UpdateProjectAsync(ProjectModel project)
    {
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
        try
        {
            var project = await context.Projects.FirstOrDefaultAsync(p => p.Id == id);
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
        await using var context = await factory.CreateDbContextAsync();
        return await context.Projects.AnyAsync(p => p.Id == id);
    }

    /// <summary>
    /// 添加成员到项目
    /// </summary>
    public async Task<bool> AddStaffToProjectAsync(string projectId, string userId)
    {
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
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