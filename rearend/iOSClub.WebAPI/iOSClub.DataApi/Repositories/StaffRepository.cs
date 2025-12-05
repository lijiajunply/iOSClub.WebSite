using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.Data.ShowModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

/// <summary>
/// 员工仓库接口，提供员工数据的CRUD操作和查询功能
/// </summary>
public interface IStaffRepository
{
    /// <summary>
    /// 获取所有员工
    /// </summary>
    /// <returns>员工列表</returns>
    Task<IEnumerable<StaffModel>> GetAllStaffAsync();
    
    /// <summary>
    /// 获取所有员工并转换为成员模型
    /// </summary>
    /// <returns>成员模型列表</returns>
    Task<IEnumerable<MemberModel>> GetAllStaffToMembers();
    
    /// <summary>
    /// 根据ID获取员工
    /// </summary>
    /// <param name="userId">员工ID</param>
    /// <returns>员工模型，如果找不到则返回null</returns>
    Task<StaffModel?> GetStaffByIdAsync(string userId);
    
    /// <summary>
    /// 根据ID获取员工，不包含关联数据
    /// </summary>
    /// <param name="userId">员工ID</param>
    /// <returns>员工模型，如果找不到则返回null</returns>
    Task<StaffModel?> GetStaffByIdWithoutOtherData(string userId);
    
    /// <summary>
    /// 创建员工
    /// </summary>
    /// <param name="staff">员工模型</param>
    /// <returns>是否创建成功</returns>
    /// <exception cref="ArgumentException">当员工身份和部门不匹配时抛出</exception>
    Task<bool> CreateStaffAsync(StaffModel staff);
    
    /// <summary>
    /// 更新员工
    /// </summary>
    /// <param name="staff">员工模型</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateStaffAsync(StaffModel staff);
    
    /// <summary>
    /// 删除员工
    /// </summary>
    /// <param name="userId">员工ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteStaffAsync(string userId);
    
    /// <summary>
    /// 检查员工是否存在
    /// </summary>
    /// <param name="userId">员工ID</param>
    /// <returns>员工是否存在</returns>
    Task<bool> StaffExistsAsync(string userId);
    
    /// <summary>
    /// 根据身份获取员工
    /// </summary>
    /// <param name="identities">身份列表</param>
    /// <returns>员工列表</returns>
    Task<IEnumerable<StaffModel>> GetStaffsByIdentitiesAsync(params string[] identities);
    
    /// <summary>
    /// 更改员工部门
    /// </summary>
    /// <param name="userId">员工ID</param>
    /// <param name="departmentName">部门名称，可为null表示移除部门</param>
    /// <returns>是否更改成功</returns>
    Task<bool> ChangeStaffDepartmentAsync(string userId, string? departmentName);
    
    /// <summary>
    /// 获取所有员工及其身份
    /// </summary>
    /// <returns>成员模型列表，包含身份信息</returns>
    Task<IEnumerable<MemberModel>> GetAllStaffIdentity();
}

public class StaffRepository(IDbContextFactory<ClubContext> factory) : IStaffRepository
{
    public async Task<IEnumerable<StaffModel>> GetAllStaffAsync()
    {
        await using var context = await factory.CreateDbContextAsync();
        return (await context.Staffs
                .Include(s => s.Department)
                .Include(s => s.Projects)
                .Include(s => s.Tasks)
                .ToListAsync())
            .Select(x => x.OutputWhenOtherList());
    }

    public async Task<IEnumerable<MemberModel>> GetAllStaffToMembers()
    {
        await using var context = await factory.CreateDbContextAsync();
        var query = from staff in context.Staffs
            join student in context.Students
                on staff.UserId equals student.UserId into studentGroup
            from student in studentGroup.DefaultIfEmpty() // LEFT JOIN
            where student != null && staff.Identity != "Founder"
            select new MemberModel
            {
                UserId = staff.UserId,
                UserName = student != null ? student.UserName : staff.Name,
                Academy = student != null ? student.Academy : "",
                PoliticalLandscape = student != null ? student.PoliticalLandscape : "群众",
                Gender = student != null ? student.Gender : "",
                ClassName = student != null ? student.ClassName : "",
                PhoneNum = student != null ? student.PhoneNum : "",
                JoinTime = student != null ? student.JoinTime : DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc),
                PasswordHash = student != null ? student.PasswordHash : "",
                EMail = student != null ? student.EMail : null,
                Identity = staff.Identity
            };

        return await query.ToListAsync();
    }
    
    public async Task<IEnumerable<MemberModel>> GetAllStaffIdentity()
    {
        await using var context = await factory.CreateDbContextAsync();
        var query = from staff in context.Staffs
            join student in context.Students
                on staff.UserId equals student.UserId into studentGroup
            from student in studentGroup.DefaultIfEmpty() // LEFT JOIN
            where student != null && staff.Identity != "Founder"
            select new MemberModel
            {
                UserId = staff.UserId,
                UserName = student != null ? student.UserName : staff.Name,
                Academy = student != null ? student.Academy : "",
                PoliticalLandscape = student != null ? student.PoliticalLandscape : "群众",
                Gender = student != null ? student.Gender : "",
                ClassName = student != null ? student.ClassName : "",
                PhoneNum = student != null ? student.PhoneNum : "",
                JoinTime = student != null ? student.JoinTime : DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc),
                PasswordHash = student != null ? student.PasswordHash : "",
                EMail = student != null ? student.EMail : null,
                Identity = staff.Identity == "Minister" ? $"{staff.Department!.Name} 部长" :
                    staff.Identity == "Department" ? $"{staff.Department!.Name} 部员" :
                    staff.Identity == "President" ? "社长" :
                    staff.Identity == "Founder" ? "创始人" : staff.Identity,
            };

        return await query.ToListAsync();
    }

    public async Task<StaffModel?> GetStaffByIdWithoutOtherData(string userId)
    {
        await using var context = await factory.CreateDbContextAsync();
        var staff = await context.Staffs
            .FirstOrDefaultAsync(s => s.UserId == userId);
        return staff?.OutputWhenOtherList();
    }

    public async Task<StaffModel?> GetStaffByIdAsync(string userId)
    {
        await using var context = await factory.CreateDbContextAsync();
        var staff = await context.Staffs
            .Include(s => s.Department)
            .Include(s => s.Projects)
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.UserId == userId);
        return staff?.OutputWhenOtherList();
    }

    public async Task<bool> CreateStaffAsync(StaffModel staff)
    {
        await using var context = await factory.CreateDbContextAsync();

        if (await StaffExistsAsync(staff.UserId))
            return false;

        if (staff.Identity == "Member" ||
            (!(staff.Identity == "Founder" || staff.Identity == "President" || staff.Identity == "Minister") &&
             staff.Department == null))
            throw new ArgumentException("会员必须指定部门");

        if (staff.Department != null)
        {
            staff.Department = await context.Departments.FirstOrDefaultAsync(d => d.Name == staff.Department.Name);
        }

        context.Staffs.Add(staff);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateStaffAsync(StaffModel staff)
    {
        await using var context = await factory.CreateDbContextAsync();

        var existingStaff = await context.Staffs.FirstOrDefaultAsync(s => s.UserId == staff.UserId);
        if (existingStaff == null)
            return false;

        // 更新字段
        existingStaff.Name = staff.Name;
        existingStaff.Identity = staff.Identity;

        context.Staffs.Update(existingStaff);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteStaffAsync(string userId)
    {
        await using var context = await factory.CreateDbContextAsync();

        var staff = await context.Staffs.FirstOrDefaultAsync(s => s.UserId == userId);
        if (staff == null)
            return false;

        context.Staffs.Remove(staff);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> StaffExistsAsync(string userId)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Staffs.AnyAsync(s => s.UserId == userId);
    }

    public async Task<IEnumerable<StaffModel>> GetStaffsByIdentitiesAsync(params string[] identities)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Staffs
            .Include(s => s.Department)
            .Where(s => ((IEnumerable<string>)identities).Contains(s.Identity))
            .ToListAsync();
    }

    public async Task<bool> ChangeStaffDepartmentAsync(string userId, string? departmentName)
    {
        await using var context = await factory.CreateDbContextAsync();

        var staff = await context.Staffs
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (staff == null)
            return false;

        if (string.IsNullOrEmpty(departmentName))
        {
            staff.Department = null;
        }
        else
        {
            var department = await context.Departments.FirstOrDefaultAsync(d => d.Name == departmentName);
            if (department == null)
                return false;

            staff.Department = department;
        }

        return await context.SaveChangesAsync() > 0;
    }
}