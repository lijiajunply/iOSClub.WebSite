using iOSClub.Data;
using iOSClub.DataApi.Repositories;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace iOSClub.Tests;

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
        
        _context = new ClubContext(_options);
        _departmentRepository = new DepartmentRepository(_context);
    }

    [Fact]
    public async Task GetAllDepartmentsAsync_ReturnsAllDepartments()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var departments = new List<DepartmentModel>
        {
            new() { Key = "dept1", Name = "Department 1", Description = "Description 1" },
            new() { Key = "dept2", Name = "Department 2", Description = "Description 2" }
        };
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
        
        var department = new DepartmentModel { Key = "dept1", Name = "Test Department", Description = "Test Description" };
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();

        // Act
        var result = await _departmentRepository.GetDepartmentByNameAsync("Test Department");

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
        
        var department = new DepartmentModel { Key = "test-key", Name = "Test Department", Description = "Test Description" };
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();

        // Act
        var result = await _departmentRepository.GetDepartmentByKeyAsync("test-key");

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
        
        var department = new DepartmentModel { Key = "dept1", Name = "New Department", Description = "New Description" };

        // Act
        var result = await _departmentRepository.AddDepartmentAsync(department);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var savedDepartment = await newContext.Departments.FirstOrDefaultAsync(d => d.Name == "New Department");

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
        
        var department = new DepartmentModel { Key = "dept1", Name = "Test Department", Description = "Original Description" };
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();
        
        // 修改部门信息
        department.Description = "Updated Description";

        // Act
        var result = await _departmentRepository.UpdateDepartmentAsync(department);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var updatedDepartment = await newContext.Departments.FirstOrDefaultAsync(d => d.Name == "Test Department");

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
        
        var department = new DepartmentModel { Key = "dept1", Name = "Test Department", Description = "Test Description" };
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();

        // Act
        var result = await _departmentRepository.DeleteDepartmentAsync("Test Department");
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var deletedDepartment = await newContext.Departments.FirstOrDefaultAsync(d => d.Name == "Test Department");

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
        
        var department = new DepartmentModel { Key = "dept1", Name = "Test Department", Description = "Test Description" };
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();

        // Act
        var result = await _departmentRepository.DepartmentExistsAsync("Test Department");

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
        
        var department = new DepartmentModel { Key = "dept1", Name = "Test Department", Description = "Test Description" };
        await _context.Departments.AddAsync(department);
        
        var staffs = new List<StaffModel>
        {
            new() { UserId = "staff1", Name = "Staff 1", Identity = "Member", Department = department },
            new() { UserId = "staff2", Name = "Staff 2", Identity = "Member", Department = department }
        };
        await _context.Staffs.AddRangeAsync(staffs);
        await _context.SaveChangesAsync();

        // Act
        var result = await _departmentRepository.GetStaffCountAsync("Test Department");

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetProjectCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var department = new DepartmentModel { Key = "dept1", Name = "Test Department", Description = "Test Description" };
        await _context.Departments.AddAsync(department);
        
        var projects = new List<ProjectModel>
        {
            new() { Id = "project1", Title = "Project 1", Description = "Description 1", Department = department },
            new() { Id = "project2", Title = "Project 2", Description = "Description 2", Department = department }
        };
        await _context.Projects.AddRangeAsync(projects);
        await _context.SaveChangesAsync();

        // Act
        var result = await _departmentRepository.GetProjectCountAsync("Test Department");

        // Assert
        Assert.Equal(2, result);
    }
}
