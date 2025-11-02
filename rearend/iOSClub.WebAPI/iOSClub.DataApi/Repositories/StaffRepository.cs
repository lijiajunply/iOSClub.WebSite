using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.ShowModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

public interface IStaffRepository
{
    Task<IEnumerable<StaffModel>> GetAllStaffAsync();
    Task<IEnumerable<MemberModel>> GetAllStaffToMembers();
    Task<StaffModel?> GetStaffByIdAsync(string userId);
    Task<bool> CreateStaffAsync(StaffModel staff);
    Task<bool> UpdateStaffAsync(StaffModel staff);
    Task<bool> DeleteStaffAsync(string userId);
    Task<bool> StaffExistsAsync(string userId);
    Task<IEnumerable<StaffModel>> GetStaffsByIdentitiesAsync(params string[] identities);
    Task<bool> ChangeStaffDepartmentAsync(string userId, string? departmentName);
}

public class StaffRepository(IDbContextFactory<iOSContext> factory) : IStaffRepository
{
    public async Task<IEnumerable<StaffModel>> GetAllStaffAsync()
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Staffs
            .Include(s => s.Department)
            .Include(s => s.Projects)
            .Include(s => s.Tasks)
            .ToListAsync();
    }

    public async Task<IEnumerable<MemberModel>> GetAllStaffToMembers()
    {
        await using var context = await factory.CreateDbContextAsync();
        var query = from staff in context.Staffs
            join student in context.Students
                on staff.UserId equals student.UserId into studentGroup
            from student in studentGroup.DefaultIfEmpty() // LEFT JOIN
            select new MemberModel
            {
                UserId = staff.UserId,
                UserName = student != null ? student.UserName : staff.Name,
                Academy = student != null ? student.Academy : "",
                PoliticalLandscape = student != null ? student.PoliticalLandscape : "群众",
                Gender = student != null ? student.Gender : "",
                ClassName = student != null ? student.ClassName : "",
                PhoneNum = student != null ? student.PhoneNum : "",
                JoinTime = student != null ? student.JoinTime : DateTime.Today,
                PasswordHash = student != null ? student.PasswordHash : "",
                EMail = student != null ? student.EMail : null,
                Identity = staff.Identity
            };
        
        return await query.ToListAsync();
    }

    public async Task<StaffModel?> GetStaffByIdAsync(string userId)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Staffs
            .Include(s => s.Department)
            .Include(s => s.Projects)
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<bool> CreateStaffAsync(StaffModel staff)
    {
        await using var context = await factory.CreateDbContextAsync();

        if (await StaffExistsAsync(staff.UserId))
            return false;

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
            .Where(s => identities.Contains(s.Identity))
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