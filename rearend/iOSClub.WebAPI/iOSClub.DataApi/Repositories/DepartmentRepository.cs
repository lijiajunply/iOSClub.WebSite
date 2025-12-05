using iOSClub.Data;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

public interface IDepartmentRepository
{
    Task<List<DepartmentModel>> GetAllDepartmentsAsync();
    Task<DepartmentModel?> GetDepartmentByNameAsync(string name);
    Task<DepartmentModel?> GetDepartmentByKeyAsync(string key);
    Task<bool> AddDepartmentAsync(DepartmentModel department);
    Task<bool> UpdateDepartmentAsync(DepartmentModel department);
    Task<bool> DeleteDepartmentAsync(string name);
}

public class DepartmentRepository(ClubContext context) : IDepartmentRepository
{
    /// <summary>
    /// 获取所有部门
    /// </summary>
    public async Task<List<DepartmentModel>> GetAllDepartmentsAsync()
    {
        return (await context.Departments
            .Include(d => d.Staffs)
            .Include(d => d.Projects)
            .ToListAsync())
            .Select(x => x.OutputWhenOtherList())
            .ToList();
    }

    /// <summary>
    /// 根据部门名称获取部门
    /// </summary>
    public async Task<DepartmentModel?> GetDepartmentByNameAsync(string name)
    {
        var department = await context.Departments
            .Include(d => d.Staffs)
            .Include(d => d.Projects)
            .FirstOrDefaultAsync(d => d.Name == name);
        return department?.OutputWhenOtherList();
    }

    /// <summary>
    /// 根据部门Key获取部门
    /// </summary>
    public async Task<DepartmentModel?> GetDepartmentByKeyAsync(string key)
    {
        var department = await context.Departments
            .Include(d => d.Staffs)
            .Include(d => d.Projects)
            .FirstOrDefaultAsync(d => d.Key == key);
        return department?.OutputWhenOtherList();
    }

    /// <summary>
    /// 添加新部门
    /// </summary>
    public async Task<bool> AddDepartmentAsync(DepartmentModel department)
    {
        try
        {
            await context.Departments.AddAsync(department);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 更新部门信息
    /// </summary>
    public async Task<bool> UpdateDepartmentAsync(DepartmentModel department)
    {
        try
        {
            context.Departments.Update(department);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 删除部门
    /// </summary>
    public async Task<bool> DeleteDepartmentAsync(string name)
    {
        try
        {
            var department = await GetDepartmentByNameAsync(name);
            if (department == null)
                return false;

            context.Departments.Remove(department);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查部门是否存在
    /// </summary>
    public async Task<bool> DepartmentExistsAsync(string name)
    {
        return await context.Departments.AnyAsync(d => d.Name == name);
    }

    /// <summary>
    /// 获取部门的成员数量
    /// </summary>
    public async Task<int> GetStaffCountAsync(string departmentName)
    {
        return await context.Departments
            .Where(d => d.Name == departmentName)
            .SelectMany(d => d.Staffs)
            .CountAsync();
    }

    /// <summary>
    /// 获取部门的项目数量
    /// </summary>
    public async Task<int> GetProjectCountAsync(string departmentName)
    {
        return await context.Departments
            .Where(d => d.Name == departmentName)
            .SelectMany(d => d.Projects)
            .CountAsync();
    }

    /// <summary>
    /// 获取指定部门的所有成员
    /// </summary>
    public async Task<List<StaffModel>> GetDepartmentStaffsAsync(string departmentName)
    {
        return await context.Departments
            .Where(d => d.Name == departmentName)
            .SelectMany(d => d.Staffs)
            .ToListAsync();
    }

    /// <summary>
    /// 获取指定部门的所有项目
    /// </summary>
    public async Task<List<ProjectModel>> GetDepartmentProjectsAsync(string departmentName)
    {
        return await context.Departments
            .Where(d => d.Name == departmentName)
            .SelectMany(d => d.Projects)
            .ToListAsync();
    }
}