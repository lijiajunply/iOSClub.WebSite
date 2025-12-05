using iOSClub.Data;
using iOSClub.DataApi.Services;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.Tests;

public class DataCentreServiceTests
{
    private readonly DbContextOptions<ClubContext> _options;
    private readonly DataCentreService _dataCentreService;

    public DataCentreServiceTests()
    {
        // 使用内存数据库进行测试，而不是复杂的Moq设置
        _options = new DbContextOptionsBuilder<ClubContext>()
            .UseInMemoryDatabase(databaseName: "DataCentreTestDatabase")
            .Options;

        var contextFactory = new TestDbContextFactory(_options);
        _dataCentreService = new DataCentreService(contextFactory);
    }

    [Fact]
    public async Task GetGenderDataAsync_ReturnsCorrectGenderCounts()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var students = new List<StudentModel>
        {
            new() { UserId = "2101010101", Gender = "男" },
            new() { UserId = "2101010102", Gender = "女" },
            new() { UserId = "2101010103", Gender = "男" },
            new() { UserId = "2101010104", Gender = "男" },
            new() { UserId = "2101010105", Gender = "女" }
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // Act
        var result = await _dataCentreService.GetGenderDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(3, result.First(r => r.Type == "男").Value);
        Assert.Equal(2, result.First(r => r.Type == "女").Value);
    }

    [Fact]
    public async Task GetCollegeDataAsync_ReturnsCorrectAcademyCounts()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var students = new List<StudentModel>
        {
            new() { UserId = "2101010101", Academy = "计算机学院" },
            new() { UserId = "2101010102", Academy = "电子工程学院" },
            new() { UserId = "2101010103", Academy = "计算机学院" },
            new() { UserId = "2101010104", Academy = "机械工程学院" },
            new() { UserId = "2101010105", Academy = "计算机学院" },
            new() { UserId = "2101010106", Academy = "电子工程学院" }
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // Act
        var result = await _dataCentreService.GetCollegeDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("计算机学院", result[0].Type);
        Assert.Equal(3, result[0].Value);
        Assert.Equal("电子工程学院", result[1].Type);
        Assert.Equal(2, result[1].Value);
    }

    [Fact]
    public async Task GetGradeDataAsync_ReturnsCorrectGradeCounts()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var students = new List<StudentModel>
        {
            new() { UserId = "2101010101" },
            new() { UserId = "2101010102" },
            new() { UserId = "2201010103" },
            new() { UserId = "2201010104" },
            new() { UserId = "2201010105" },
            new() { UserId = "2301010106" }
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // Act
        var result = await _dataCentreService.GetGradeDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, r => r.Grade == "21级" && r.Value == 2);
        Assert.Contains(result, r => r.Grade == "22级" && r.Value == 3);
        Assert.Contains(result, r => r.Grade == "23级" && r.Value == 1);
    }
}

// 简单的DbContextFactory实现，用于测试
public class TestDbContextFactory(DbContextOptions<ClubContext> options) : IDbContextFactory<ClubContext>
{
    public ClubContext CreateDbContext()
    {
        return new ClubContext(options);
    }

    public ValueTask<ClubContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(new ClubContext(options));
    }
}