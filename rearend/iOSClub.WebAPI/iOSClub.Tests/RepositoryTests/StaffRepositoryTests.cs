using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.Tests.RepositoryTests;

public class StaffRepositoryTests
{
    private readonly DbContextOptions<ClubContext> _options;
    private readonly StaffRepository _staffRepository;

    public StaffRepositoryTests()
    {
        // 使用内存数据库进行测试
        _options = new DbContextOptionsBuilder<ClubContext>()
            .UseInMemoryDatabase(databaseName: "StaffRepositoryTestDatabase")
            .Options;
        
        var contextFactory = new TestDbContextFactory(_options);
        _staffRepository = new StaffRepository(contextFactory);
    }

    [Fact]
    public async Task GetAllStaffAsync_ReturnsAllStaffMembers()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var staffs = new List<StaffModel>
        {
            new() { UserId = "staff1", Name = "Staff 1", Identity = "Member" },
            new() { UserId = "staff2", Name = "Staff 2", Identity = "Member" }
        };
        await context.Staffs.AddRangeAsync(staffs);
        await context.SaveChangesAsync();

        // Act
        var result = await _staffRepository.GetAllStaffAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetStaffByIdAsync_ReturnsCorrectStaff()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var staff = new StaffModel { UserId = "staff1", Name = "Test Staff", Identity = "Member" };
        await context.Staffs.AddAsync(staff);
        await context.SaveChangesAsync();

        // Act
        var result = await _staffRepository.GetStaffByIdAsync("staff1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(staff.UserId, result.UserId);
        Assert.Equal(staff.Name, result.Name);
    }

    [Fact]
    public async Task GetStaffByIdWithoutOtherData_ReturnsCorrectStaff()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var staff = new StaffModel { UserId = "staff1", Name = "Test Staff", Identity = "Member" };
        await context.Staffs.AddAsync(staff);
        await context.SaveChangesAsync();

        // Act
        var result = await _staffRepository.GetStaffByIdWithoutOtherData("staff1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(staff.UserId, result.UserId);
        Assert.Equal(staff.Name, result.Name);
    }

    [Fact]
    public async Task StaffExistsAsync_ReturnsTrue_WhenStaffExists()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var staff = new StaffModel { UserId = "staff1", Name = "Test Staff", Identity = "Member" };
        await context.Staffs.AddAsync(staff);
        await context.SaveChangesAsync();

        // Act
        var result = await _staffRepository.StaffExistsAsync("staff1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task StaffExistsAsync_ReturnsFalse_WhenStaffDoesNotExist()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Act
        var result = await _staffRepository.StaffExistsAsync("non-existent-staff");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateStaffAsync_CreatesNewStaff()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 创建一个部门，因为Member身份的员工需要部门
        var department = new DepartmentModel { Key = "dept1", Name = "Test Department", Description = "Test Description" };
        await context.Departments.AddAsync(department);
        await context.SaveChangesAsync();
        
        // 创建一个Founder身份的员工，这样不需要部门
        var staff = new StaffModel { UserId = "staff1", Name = "New Staff", Identity = "Founder" };

        // Act
        var result = await _staffRepository.CreateStaffAsync(staff);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var savedStaff = await newContext.Staffs.FirstOrDefaultAsync(s => s.UserId == "staff1");

        // Assert
        Assert.True(result);
        Assert.NotNull(savedStaff);
        Assert.Equal(staff.UserId, savedStaff.UserId);
    }

    [Fact]
    public async Task UpdateStaffAsync_UpdatesExistingStaff()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var staff = new StaffModel { UserId = "staff1", Name = "Original Name", Identity = "Member" };
        await context.Staffs.AddAsync(staff);
        await context.SaveChangesAsync();
        
        // 修改员工信息
        staff.Name = "Updated Name";
        staff.Identity = "Minister";

        // Act
        var result = await _staffRepository.UpdateStaffAsync(staff);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var updatedStaff = await newContext.Staffs.FirstOrDefaultAsync(s => s.UserId == "staff1");

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedStaff);
        Assert.Equal("Updated Name", updatedStaff.Name);
        Assert.Equal("Minister", updatedStaff.Identity);
    }

    [Fact]
    public async Task DeleteStaffAsync_RemovesStaff()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var staff = new StaffModel { UserId = "staff1", Name = "Test Staff", Identity = "Member" };
        await context.Staffs.AddAsync(staff);
        await context.SaveChangesAsync();

        // Act
        var result = await _staffRepository.DeleteStaffAsync("staff1");
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var deletedStaff = await newContext.Staffs.FirstOrDefaultAsync(s => s.UserId == "staff1");

        // Assert
        Assert.True(result);
        Assert.Null(deletedStaff);
    }

    [Fact]
    public async Task ChangeStaffDepartmentAsync_ChangesStaffDepartment()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var department1 = new DepartmentModel { Key = "dept1", Name = "Department 1", Description = "Description 1" };
        var department2 = new DepartmentModel { Key = "dept2", Name = "Department 2", Description = "Description 2" };
        await context.Departments.AddRangeAsync(department1, department2);
        
        var staff = new StaffModel { UserId = "staff1", Name = "Test Staff", Identity = "Member", Department = department1 };
        await context.Staffs.AddAsync(staff);
        await context.SaveChangesAsync();

        // Act
        var result = await _staffRepository.ChangeStaffDepartmentAsync("staff1", "Department 2");
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var updatedStaff = await newContext.Staffs.Include(s => s.Department).FirstOrDefaultAsync(s => s.UserId == "staff1");

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedStaff);
        Assert.NotNull(updatedStaff.Department);
        Assert.Equal("Department 2", updatedStaff.Department.Name);
    }

    [Fact]
    public async Task GetStaffsByIdentitiesAsync_ReturnsStaffsWithMatchingIdentities()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var staffs = new List<StaffModel>
        {
            new() { UserId = "staff1", Name = "Staff 1", Identity = "President" },
            new() { UserId = "staff2", Name = "Staff 2", Identity = "Minister" },
            new() { UserId = "staff3", Name = "Staff 3", Identity = "Member" }
        };
        await context.Staffs.AddRangeAsync(staffs);
        await context.SaveChangesAsync();

        // Act
        var result = await _staffRepository.GetStaffsByIdentitiesAsync("President", "Minister");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, s => Assert.Contains(s.Identity, new[] { "President", "Minister" }));
    }
}
