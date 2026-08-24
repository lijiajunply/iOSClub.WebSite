using iOSClub.Data;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.Tests;

public class DataLayerTests
{
    private readonly DbContextOptions<ClubContext> _options = new DbContextOptionsBuilder<ClubContext>()
        .UseInMemoryDatabase(databaseName: "TestDatabase")
        .Options;

    // 使用内存数据库进行测试

    [Fact]
    public async Task CanAddAndRetrieveStudent()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        var student = BogusDataGenerator.StudentFaker.Clone()
            .RuleFor(s => s.UserId, "1234567890")
            .RuleFor(s => s.JoinTime, DateTime.UtcNow)
            .Generate();

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
        var category = BogusDataGenerator.CategoryFaker.Clone()
            .RuleFor(c => c.Id, "test-category")
            .RuleFor(c => c.Order, 1)
            .Generate();
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var article = BogusDataGenerator.ArticleFaker.Clone()
            .RuleFor(a => a.Path, "/test/article")
            .RuleFor(a => a.CategoryId, category.Id)
            .RuleFor(a => a.LastWriteTime, DateTime.UtcNow)
            .RuleFor(a => a.ArticleOrder, 1)
            .Generate();

        // Act
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        // Assert
        var retrievedArticle = await context.Articles.FindAsync(article.Path);
        Assert.NotNull(retrievedArticle);
        Assert.Equal(article.Title, retrievedArticle.Title);
        Assert.Equal(category.Id, retrievedArticle.CategoryId);
    }

}
