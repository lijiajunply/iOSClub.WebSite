using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.Tests.RepositoryTests;

public class DepartmentRepositoryTests
{
    private readonly DbContextOptions<ClubContext> _options;
    private readonly ClubContext _context;
    private readonly DepartmentRepository _departmentRepository;

    public DepartmentRepositoryTests()
    {
        // 使用内存数据库进行测试
        _options = new DbContextOptionsBuilder<ClubContext>()
            .UseInMemoryDatabase(databaseName: "DepartmentRepositoryTestDatabase")
            .Options;
        
        var contextFactory = new TestDbContextFactory(_options);
        _departmentRepository = new DepartmentRepository(contextFactory);
        
        // 初始化上下文
        _context = contextFactory.CreateDbContext();
    }

    [Fact]
    public async Task GetAllDepartmentsAsync_ReturnsAllDepartments()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成2个部门
        var departments = BogusDataGenerator.GenerateDepartments(2);
        await _context.Departments.AddRangeAsync(departments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _departmentRepository.GetAllDepartmentsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetDepartmentByNameAsync_ReturnsCorrectDepartment()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个部门
        var department = BogusDataGenerator.DepartmentFaker.Generate();
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();

        // Act
        var result = await _departmentRepository.GetDepartmentByNameAsync(department.Name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(department.Name, result.Name);
        Assert.Equal(department.Description, result.Description);
    }

    [Fact]
    public async Task GetDepartmentByKeyAsync_ReturnsCorrectDepartment()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个部门
        var department = BogusDataGenerator.DepartmentFaker.Generate();
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();

        // Act
        var result = await _departmentRepository.GetDepartmentByKeyAsync(department.Key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(department.Key, result.Key);
        Assert.Equal(department.Name, result.Name);
    }

    [Fact]
    public async Task AddDepartmentAsync_AddsNewDepartment()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个部门
        var department = BogusDataGenerator.DepartmentFaker.Generate();

        // Act
        var result = await _departmentRepository.AddDepartmentAsync(department);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var savedDepartment = await newContext.Departments.FirstOrDefaultAsync(d => d.Key == department.Key);

        // Assert
        Assert.True(result);
        Assert.NotNull(savedDepartment);
        Assert.Equal(department.Name, savedDepartment.Name);
    }

    [Fact]
    public async Task UpdateDepartmentAsync_UpdatesExistingDepartment()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个部门
        var department = BogusDataGenerator.DepartmentFaker.Generate();
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();
        
        // 修改部门信息
        department.Description = "Updated Description";

        // Act
        var result = await _departmentRepository.UpdateDepartmentAsync(department);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var updatedDepartment = await newContext.Departments.FirstOrDefaultAsync(d => d.Key == department.Key);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedDepartment);
        Assert.Equal("Updated Description", updatedDepartment.Description);
    }

    [Fact]
    public async Task DeleteDepartmentAsync_RemovesDepartment()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个部门
        var department = BogusDataGenerator.DepartmentFaker.Generate();
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();

        // Act
        var result = await _departmentRepository.DeleteDepartmentAsync(department.Name);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var deletedDepartment = await newContext.Departments.FirstOrDefaultAsync(d => d.Name == department.Name);

        // Assert
        Assert.True(result);
        Assert.Null(deletedDepartment);
    }

    [Fact]
    public async Task DepartmentExistsAsync_ReturnsTrue_WhenDepartmentExists()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个部门
        var department = BogusDataGenerator.DepartmentFaker.Generate();
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();

        // Act
        var result = await _departmentRepository.DepartmentExistsAsync(department.Name);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DepartmentExistsAsync_ReturnsFalse_WhenDepartmentDoesNotExist()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        // Act
        var result = await _departmentRepository.DepartmentExistsAsync("NonExistentDepartment");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetStaffCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个部门
        var department = BogusDataGenerator.DepartmentFaker.Generate();
        await _context.Departments.AddAsync(department);
        
        // 使用Bogus生成2个员工
        var staffs = BogusDataGenerator.GenerateStaffs(2);
        foreach (var staff in staffs)
        {
            staff.Department = department;
        }
        await _context.Staffs.AddRangeAsync(staffs);
        await _context.SaveChangesAsync();

        // Act
        var result = await _departmentRepository.GetStaffCountAsync(department.Name);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetProjectCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个部门
        var department = BogusDataGenerator.DepartmentFaker.Generate();
        await _context.Departments.AddAsync(department);
        
        // 使用Bogus生成2个项目
        var projects = BogusDataGenerator.GenerateProjects(2);
        foreach (var project in projects)
        {
            project.Department = department;
        }
        await _context.Projects.AddRangeAsync(projects);
        await _context.SaveChangesAsync();

        // Act
        var result = await _departmentRepository.GetProjectCountAsync(department.Name);

        // Assert
        Assert.Equal(2, result);
    }
}
