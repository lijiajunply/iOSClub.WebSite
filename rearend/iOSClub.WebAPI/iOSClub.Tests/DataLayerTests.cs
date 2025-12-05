using iOSClub.Data;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace iOSClub.Tests;

public class DataLayerTests
{
    private readonly DbContextOptions<ClubContext> _options;

    public DataLayerTests()
    {
        // 使用内存数据库进行测试
        _options = new DbContextOptionsBuilder<ClubContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
    }

    [Fact]
    public async Task CanAddAndRetrieveStudent()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        var student = new StudentModel
        {
            UserId = "1234567890",
            UserName = "Test Student",
            EMail = "test@example.com",
            PhoneNum = "13800138000",
            Academy = "Computer Science",
            ClassName = "CS2022",
            Gender = "男",
            PoliticalLandscape = "群众",
            JoinTime = DateTime.UtcNow
        };

        // Act
        context.Students.Add(student);
        await context.SaveChangesAsync();

        // Assert
        var retrievedStudent = await context.Students.FindAsync(student.UserId);
        Assert.NotNull(retrievedStudent);
        Assert.Equal(student.UserName, retrievedStudent.UserName);
        Assert.Equal(student.EMail, retrievedStudent.EMail);
    }

    [Fact]
    public async Task CanAddAndRetrieveArticle()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        var category = new CategoryModel
        {
            Id = "test-category",
            Name = "Test Category",
            Description = "Test Description",
            Order = 1
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var article = new ArticleModel
        {
            Path = "/test/article",
            Title = "Test Article",
            Content = "Test Content",
            CategoryId = category.Id,
            Identity = "Test Author",
            LastWriteTime = DateTime.UtcNow,
            ArticleOrder = 1
        };

        // Act
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        // Assert
        var retrievedArticle = await context.Articles.FindAsync(article.Path);
        Assert.NotNull(retrievedArticle);
        Assert.Equal(article.Title, retrievedArticle.Title);
        Assert.Equal(category.Id, retrievedArticle.CategoryId);
    }

    [Fact]
    public async Task CanAddAndRetrieveProject()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        var department = new DepartmentModel
        {
            Name = "Test Department",
            Key = "test-dept",
            Description = "Test Description"
        };
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        var project = new ProjectModel
        {
            Id = "test-project",
            Title = "Test Project",
            Description = "Test Description",
            StartTime = "2024-01-01",
            EndTime = "2024-12-31"
        };

        // Act
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        // Assert
        var retrievedProject = await context.Projects.FindAsync(project.Id);
        Assert.NotNull(retrievedProject);
        Assert.Equal(project.Title, retrievedProject.Title);
        Assert.Equal(project.Description, retrievedProject.Description);
    }
}
