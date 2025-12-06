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
        
        // 使用Bogus生成2个员工
        var staffs = BogusDataGenerator.GenerateStaffs(2);
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
        
        // 使用Bogus生成1个员工
        var staff = BogusDataGenerator.StaffFaker.Generate();
        await context.Staffs.AddAsync(staff);
        await context.SaveChangesAsync();

        // Act
        var result = await _staffRepository.GetStaffByIdAsync(staff.UserId);

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
        
        // 使用Bogus生成1个员工
        var staff = BogusDataGenerator.StaffFaker.Generate();
        await context.Staffs.AddAsync(staff);
        await context.SaveChangesAsync();

        // Act
        var result = await _staffRepository.GetStaffByIdWithoutOtherData(staff.UserId);

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
        
        // 使用Bogus生成1个员工
        var staff = BogusDataGenerator.StaffFaker.Generate();
        await context.Staffs.AddAsync(staff);
        await context.SaveChangesAsync();

        // Act
        var result = await _staffRepository.StaffExistsAsync(staff.UserId);

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
        
        // 使用Bogus生成1个Founder身份的员工
        var staff = BogusDataGenerator.StaffFaker.Generate();
        staff.Identity = "Founder"; // Founder身份不需要部门

        // Act
        var result = await _staffRepository.CreateStaffAsync(staff);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var savedStaff = await newContext.Staffs.FirstOrDefaultAsync(s => s.UserId == staff.UserId);

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
        
        // 使用Bogus生成1个员工
        var staff = BogusDataGenerator.StaffFaker.Generate();
        await context.Staffs.AddAsync(staff);
        await context.SaveChangesAsync();
        
        // 修改员工信息
        staff.Name = "Updated Name";
        staff.Identity = "Minister";

        // Act
        var result = await _staffRepository.UpdateStaffAsync(staff);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var updatedStaff = await newContext.Staffs.FirstOrDefaultAsync(s => s.UserId == staff.UserId);

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
        
        // 使用Bogus生成1个员工
        var staff = BogusDataGenerator.StaffFaker.Generate();
        await context.Staffs.AddAsync(staff);
        await context.SaveChangesAsync();

        // Act
        var result = await _staffRepository.DeleteStaffAsync(staff.UserId);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var deletedStaff = await newContext.Staffs.FirstOrDefaultAsync(s => s.UserId == staff.UserId);

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
        
        // 使用Bogus生成2个部门
        var departments = BogusDataGenerator.GenerateDepartments(2);
        await context.Departments.AddRangeAsync(departments);
        
        // 使用Bogus生成1个员工，并关联到第一个部门
        var staff = BogusDataGenerator.StaffFaker.Generate();
        staff.Department = departments[0];
        await context.Staffs.AddAsync(staff);
        await context.SaveChangesAsync();

        // Act
        var result = await _staffRepository.ChangeStaffDepartmentAsync(staff.UserId, departments[1].Name);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var updatedStaff = await newContext.Staffs.Include(s => s.Department).FirstOrDefaultAsync(s => s.UserId == staff.UserId);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedStaff);
        Assert.NotNull(updatedStaff.Department);
        Assert.Equal(departments[1].Name, updatedStaff.Department.Name);
    }

    [Fact]
    public async Task GetStaffsByIdentitiesAsync_ReturnsStaffsWithMatchingIdentities()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成3个员工，分别设置不同身份
        var presidentStaff = BogusDataGenerator.StaffFaker.Generate();
        presidentStaff.Identity = "President";
        
        var ministerStaff = BogusDataGenerator.StaffFaker.Generate();
        ministerStaff.Identity = "Minister";
        
        var memberStaff = BogusDataGenerator.StaffFaker.Generate();
        memberStaff.Identity = "Member";
        
        var staffs = new List<StaffModel> { presidentStaff, ministerStaff, memberStaff };
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
