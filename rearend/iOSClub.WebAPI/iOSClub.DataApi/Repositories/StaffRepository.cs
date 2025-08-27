using iOSClub.Data;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

public interface IStaffRepository
{
    Task<IEnumerable<StaffModel>> GetAllStaffAsync();
    Task<StaffModel?> GetStaffByIdAsync(string userId);
    Task<bool> CreateStaffAsync(StaffModel staff);
    Task<bool> UpdateStaffAsync(StaffModel staff);
    Task<bool> DeleteStaffAsync(string userId);
    Task<bool> StaffExistsAsync(string userId);
    Task<IEnumerable<StaffModel>> GetStaffsByIdentitiesAsync(params string[] identities);
    Task<bool> ChangeStaffDepartmentAsync(string userId, string? departmentName);
}

public class StaffRepository : IStaffRepository
{
    private readonly IDbContextFactory<iOSContext> _factory;

    public StaffRepository(IDbContextFactory<iOSContext> factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<StaffModel>> GetAllStaffAsync()
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.Staffs
            .Include(s => s.Department)
            .Include(s => s.Projects)
            .Include(s => s.Tasks)
            .ToListAsync();
    }

    public async Task<StaffModel?> GetStaffByIdAsync(string userId)
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.Staffs
            .Include(s => s.Department)
            .Include(s => s.Projects)
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<bool> CreateStaffAsync(StaffModel staff)
    {
        await using var context = await _factory.CreateDbContextAsync();

        if (await StaffExistsAsync(staff.UserId))
            return false;

        context.Staffs.Add(staff);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateStaffAsync(StaffModel staff)
    {
        await using var context = await _factory.CreateDbContextAsync();

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
        await using var context = await _factory.CreateDbContextAsync();

        var staff = await context.Staffs.FirstOrDefaultAsync(s => s.UserId == userId);
        if (staff == null)
            return false;

        context.Staffs.Remove(staff);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> StaffExistsAsync(string userId)
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.Staffs.AnyAsync(s => s.UserId == userId);
    }

    public async Task<IEnumerable<StaffModel>> GetStaffsByIdentitiesAsync(params string[] identities)
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.Staffs
            .Include(s => s.Department)
            .Where(s => identities.Contains(s.Identity))
            .ToListAsync();
    }

    public async Task<bool> ChangeStaffDepartmentAsync(string userId, string? departmentName)
    {
        await using var context = await _factory.CreateDbContextAsync();

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