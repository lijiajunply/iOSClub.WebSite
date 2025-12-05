using iOSClub.Data;
using iOSClub.DataApi.Repositories;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.Tests;

public class StudentRepositoryTests
{
    private readonly DbContextOptions<ClubContext> _options;
    private readonly ClubContext _context;
    private readonly StudentRepository _studentRepository;

    public StudentRepositoryTests()
    {
        // 使用内存数据库进行测试
        _options = new DbContextOptionsBuilder<ClubContext>()
            .UseInMemoryDatabase(databaseName: "StudentRepositoryTestDatabase")
            .Options;
        
        _context = new ClubContext(_options);
        _studentRepository = new StudentRepository(_context);
    }

    [Fact]
    public async Task GetAll_ReturnsAllStudents()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var students = new List<StudentModel>
        {
            new() { UserId = "2101010101", UserName = "Student 1", PasswordHash = DataTool.StringToHash("password1"), PhoneNum = "13800138001" },
            new() { UserId = "2101010102", UserName = "Student 2", PasswordHash = DataTool.StringToHash("password2"), PhoneNum = "13800138002" }
        };
        await _context.Students.AddRangeAsync(students);
        await _context.SaveChangesAsync();

        // Act
        var result = await _studentRepository.GetAll();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectStudent()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var student = new StudentModel { UserId = "2101010101", UserName = "Test Student", PasswordHash = DataTool.StringToHash("password"), PhoneNum = "13800138001" };
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        // Act
        var result = await _studentRepository.GetByIdAsync("2101010101");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(student.UserName, result.UserName);
        Assert.Equal(student.PhoneNum, result.PhoneNum);
    }

    [Fact]
    public async Task Create_ReturnsCreatedStudent()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var student = new StudentModel { UserId = "2101010101", UserName = "New Student", PasswordHash = DataTool.StringToHash("password"), PhoneNum = "13800138001" };

        // Act
        var result = await _studentRepository.Create(student);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(student.UserId, result.UserId);
        Assert.Equal(student.UserName, result.UserName);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingStudent()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var student = new StudentModel { UserId = "2101010101", UserName = "Original Name", PasswordHash = DataTool.StringToHash("password"), PhoneNum = "13800138001" };
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();
        
        student.UserName = "Updated Name";
        student.PhoneNum = "13800138002";

        // Act
        var result = await _studentRepository.UpdateAsync(student);
        var updatedStudent = await _studentRepository.GetByIdAsync("2101010101");

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedStudent);
        Assert.Equal("Updated Name", updatedStudent.UserName);
        Assert.Equal("13800138002", updatedStudent.PhoneNum);
    }

    [Fact]
    public async Task DeleteAsync_RemovesStudent()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var student = new StudentModel { UserId = "2101010101", UserName = "Test Student", PasswordHash = DataTool.StringToHash("password"), PhoneNum = "13800138001" };
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        // Act
        var result = await _studentRepository.DeleteAsync("2101010101");
        var deletedStudent = await _studentRepository.GetByIdAsync("2101010101");

        // Assert
        Assert.True(result);
        Assert.Null(deletedStudent);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTrue()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var student = new StudentModel { UserId = "2101010101", UserName = "Test Student", PasswordHash = DataTool.StringToHash("password"), PhoneNum = "13800138001" };
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        // Act
        var result = await _studentRepository.Login("2101010101", "password");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsFalse()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var student = new StudentModel { UserId = "2101010101", UserName = "Test Student", PasswordHash = DataTool.StringToHash("password"), PhoneNum = "13800138001" };
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        // Act
        var result = await _studentRepository.Login("2101010101", "wrongpassword");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateManyAsync_AddsNewStudents()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var students = new List<StudentModel>
        {
            new() { UserId = "2101010101", UserName = "Student 1", PasswordHash = DataTool.StringToHash("password1"), PhoneNum = "13800138001" },
            new() { UserId = "2101010102", UserName = "Student 2", PasswordHash = DataTool.StringToHash("password2"), PhoneNum = "13800138002" }
        };

        // Act
        var result = await _studentRepository.UpdateManyAsync(students);
        var allStudents = await _studentRepository.GetAll();

        // Assert
        Assert.True(result);
        Assert.Equal(2, allStudents.Count);
    }

    [Fact]
    public async Task GetMembersPagedAsync_ReturnsCorrectPage()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        // 添加5个学生
        for (int i = 1; i <= 5; i++)
        {
            var student = new StudentModel 
            { 
                UserId = $"210101010{i}", 
                UserName = $"Student {i}", 
                PasswordHash = DataTool.StringToHash($"password{i}"), 
                PhoneNum = $"1380013800{i}"
            };
            await _context.Students.AddAsync(student);
        }
        await _context.SaveChangesAsync();

        // Act
        var (members, totalCount) = await _studentRepository.GetMembersPagedAsync(1, 2);

        // Assert
        Assert.NotNull(members);
        Assert.Equal(2, members.Count);
        Assert.Equal(5, totalCount);
    }

    [Fact]
    public async Task Search_ReturnsMatchingStudents()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var students = new List<StudentModel>
        {
            new() { UserId = "2101010101", UserName = "Test Student 1", Academy = "Computer Science", PasswordHash = DataTool.StringToHash("password1"), PhoneNum = "13800138001" },
            new() { UserId = "2101010102", UserName = "Test Student 2", Academy = "Mathematics", PasswordHash = DataTool.StringToHash("password2"), PhoneNum = "13800138002" },
            new() { UserId = "2101010103", UserName = "Other Student", Academy = "Computer Science", PasswordHash = DataTool.StringToHash("password3"), PhoneNum = "13800138003" }
        };
        await _context.Students.AddRangeAsync(students);
        await _context.SaveChangesAsync();

        // Act
        var result = await _studentRepository.Search("Computer Science", "academy");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, student => Assert.Equal("Computer Science", student.Academy));
    }

    [Fact]
    public async Task GetAllMembersAsync_ReturnsAllMembersWithCorrectIdentity()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        // 添加学生
        var student = new StudentModel 
        { 
            UserId = "2101010101", 
            UserName = "Test Student", 
            PasswordHash = DataTool.StringToHash("password"), 
            PhoneNum = "13800138001"
        };
        await _context.Students.AddAsync(student);
        
        // 添加员工
        var staff = new StaffModel 
        { 
            UserId = "2101010101", 
            Name = "Test Staff", 
            Identity = "President"
        };
        await _context.Staffs.AddAsync(staff);
        
        await _context.SaveChangesAsync();

        // Act
        var result = await _studentRepository.GetAllMembersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("President", result[0].Identity);
    }
}
