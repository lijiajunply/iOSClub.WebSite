using iOSClub.Data;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

/// <summary>
/// 部门仓库接口，提供部门数据的CRUD操作和查询功能
/// </summary>
public interface IDepartmentRepository
{
    /// <summary>
    /// 获取所有部门
    /// </summary>
    /// <returns>部门列表</returns>
    Task<List<DepartmentModel>> GetAllDepartmentsAsync();
    
    /// <summary>
    /// 根据名称获取部门
    /// </summary>
    /// <param name="name">部门名称</param>
    /// <returns>部门模型，如果找不到则返回null</returns>
    Task<DepartmentModel?> GetDepartmentByNameAsync(string name);
    
    /// <summary>
    /// 根据Key获取部门
    /// </summary>
    /// <param name="key">部门Key</param>
    /// <returns>部门模型，如果找不到则返回null</returns>
    Task<DepartmentModel?> GetDepartmentByKeyAsync(string key);
    
    /// <summary>
    /// 添加部门
    /// </summary>
    /// <param name="department">部门模型</param>
    /// <returns>是否添加成功</returns>
    Task<bool> AddDepartmentAsync(DepartmentModel department);
    
    /// <summary>
    /// 更新部门
    /// </summary>
    /// <param name="department">部门模型</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateDepartmentAsync(DepartmentModel department);
    
    /// <summary>
    /// 删除部门
    /// </summary>
    /// <param name="name">部门名称</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteDepartmentAsync(string name);
    
    /// <summary>
    /// 检查部门是否存在
    /// </summary>
    /// <param name="name">部门名称</param>
    /// <returns>部门是否存在</returns>
    Task<bool> DepartmentExistsAsync(string name);
    
    /// <summary>
    /// 获取部门成员数量
    /// </summary>
    /// <param name="departmentName">部门名称</param>
    /// <returns>成员数量</returns>
    Task<int> GetStaffCountAsync(string departmentName);
    
    /// <summary>
    /// 获取部门项目数量
    /// </summary>
    /// <param name="departmentName">部门名称</param>
    /// <returns>项目数量</returns>
    Task<int> GetProjectCountAsync(string departmentName);
    
    /// <summary>
    /// 获取部门成员列表
    /// </summary>
    /// <param name="departmentName">部门名称</param>
    /// <returns>成员列表</returns>
    Task<List<StaffModel>> GetDepartmentStaffsAsync(string departmentName);
    
    /// <summary>
    /// 获取部门项目列表
    /// </summary>
    /// <param name="departmentName">部门名称</param>
    /// <returns>项目列表</returns>
    Task<List<ProjectModel>> GetDepartmentProjectsAsync(string departmentName);
}

public class DepartmentRepository(IDbContextFactory<ClubContext> factory) : IDepartmentRepository
{
    /// <summary>
    /// 获取所有部门
    /// </summary>
    public async Task<List<DepartmentModel>> GetAllDepartmentsAsync()
    {
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
        return await context.Departments.AnyAsync(d => d.Name == name);
    }

    /// <summary>
    /// 获取部门的成员数量
    /// </summary>
    public async Task<int> GetStaffCountAsync(string departmentName)
    {
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();
        return await context.Departments
            .Where(d => d.Name == departmentName)
            .SelectMany(d => d.Projects)
            .ToListAsync();
    }
}