using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Services;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.Tests;

public class DataCentreServiceTests
{
    private readonly DbContextOptions<ClubContext> _options;
    private readonly TestDbContextFactory _contextFactory;
    private readonly DataCentreService _dataCentreService;

    public DataCentreServiceTests()
    {
        // 使用内存数据库进行测试
        _options = new DbContextOptionsBuilder<ClubContext>()
            .UseInMemoryDatabase(databaseName: "DataCentreServiceTestDatabase")
            .Options;
        
        _contextFactory = new TestDbContextFactory(_options);
        _dataCentreService = new DataCentreService(_contextFactory);
    }

    [Fact]
    public async Task GetYearDataAsync_ReturnsCorrectYearCounts()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // 添加测试数据
        var students = new List<StudentModel>
        {
            new() { UserId = "20123456", JoinTime = new DateTime(2020, 9, 1), Academy = "Computer", Gender = "男", PoliticalLandscape = "共青团员" },
            new() { UserId = "21123456", JoinTime = new DateTime(2021, 9, 1), Academy = "Computer", Gender = "男", PoliticalLandscape = "中共党员" },
            new() { UserId = "21234567", JoinTime = new DateTime(2021, 9, 1), Academy = "Information", Gender = "女", PoliticalLandscape = "共青团员" },
            new() { UserId = "22123456", JoinTime = new DateTime(2022, 9, 1), Academy = "Computer", Gender = "女", PoliticalLandscape = "共青团员" },
            new() { UserId = "22234567", JoinTime = new DateTime(2022, 9, 1), Academy = "Information", Gender = "男", PoliticalLandscape = "群众" },
            new() { UserId = "22345678", JoinTime = new DateTime(2022, 9, 1), Academy = "Mathematics", Gender = "男", PoliticalLandscape = "共青团员" },
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // Act
        var result = await _dataCentreService.GetYearDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, yc => yc.Year == "2019学年" || yc.Year == "2020学年" || yc.Year == "2021学年" || yc.Year == "2022学年");
    }

    [Fact]
    public async Task GetCollegeDataAsync_ReturnsCorrectAcademyCounts()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // 添加测试数据
        var students = new List<StudentModel>
        {
            new() { UserId = "20123456", JoinTime = new DateTime(2020, 9, 1), Academy = "Computer", Gender = "男", PoliticalLandscape = "共青团员" },
            new() { UserId = "21123456", JoinTime = new DateTime(2021, 9, 1), Academy = "Computer", Gender = "男", PoliticalLandscape = "中共党员" },
            new() { UserId = "21234567", JoinTime = new DateTime(2021, 9, 1), Academy = "Information", Gender = "女", PoliticalLandscape = "共青团员" },
            new() { UserId = "22123456", JoinTime = new DateTime(2022, 9, 1), Academy = "Computer", Gender = "女", PoliticalLandscape = "共青团员" },
            new() { UserId = "22234567", JoinTime = new DateTime(2022, 9, 1), Academy = "Information", Gender = "男", PoliticalLandscape = "群众" },
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // Act
        var result = await _dataCentreService.GetCollegeDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, ac => ac.Type == "Computer" && ac.Value == 3);
        Assert.Contains(result, ac => ac.Type == "Information" && ac.Value == 2);
    }

    [Fact]
    public async Task GetGradeDataAsync_ReturnsCorrectGradeCounts()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // 添加测试数据
        var students = new List<StudentModel>
        {
            new() { UserId = "20123456", JoinTime = new DateTime(2020, 9, 1), Academy = "Computer", Gender = "男", PoliticalLandscape = "共青团员" },
            new() { UserId = "21123456", JoinTime = new DateTime(2021, 9, 1), Academy = "Computer", Gender = "男", PoliticalLandscape = "中共党员" },
            new() { UserId = "21234567", JoinTime = new DateTime(2021, 9, 1), Academy = "Information", Gender = "女", PoliticalLandscape = "共青团员" },
            new() { UserId = "22123456", JoinTime = new DateTime(2022, 9, 1), Academy = "Computer", Gender = "女", PoliticalLandscape = "共青团员" },
            new() { UserId = "22234567", JoinTime = new DateTime(2022, 9, 1), Academy = "Information", Gender = "男", PoliticalLandscape = "群众" },
            new() { UserId = "22345678", JoinTime = new DateTime(2022, 9, 1), Academy = "Mathematics", Gender = "男", PoliticalLandscape = "共青团员" },
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // Act
        var result = await _dataCentreService.GetGradeDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, gc => gc.Grade == "20级" && gc.Value == 1);
        Assert.Contains(result, gc => gc.Grade == "21级" && gc.Value == 2);
        Assert.Contains(result, gc => gc.Grade == "22级" && gc.Value == 3);
    }

    [Fact]
    public async Task GetLandscapeDataAsync_ReturnsCorrectPoliticalLandscapeCounts()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // 添加测试数据
        var students = new List<StudentModel>
        {
            new() { UserId = "20123456", JoinTime = new DateTime(2020, 9, 1), Academy = "Computer", Gender = "男", PoliticalLandscape = "共青团员" },
            new() { UserId = "21123456", JoinTime = new DateTime(2021, 9, 1), Academy = "Computer", Gender = "男", PoliticalLandscape = "中共党员" },
            new() { UserId = "21234567", JoinTime = new DateTime(2021, 9, 1), Academy = "Information", Gender = "女", PoliticalLandscape = "共青团员" },
            new() { UserId = "22123456", JoinTime = new DateTime(2022, 9, 1), Academy = "Computer", Gender = "女", PoliticalLandscape = "共青团员" },
            new() { UserId = "22234567", JoinTime = new DateTime(2022, 9, 1), Academy = "Information", Gender = "男", PoliticalLandscape = "群众" },
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // Act
        var result = await _dataCentreService.GetLandscapeDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, lc => lc.Type == "共青团员" && lc.Sales == 3);
        Assert.Contains(result, lc => lc.Type == "中共党员" && lc.Sales == 1);
        Assert.Contains(result, lc => lc.Type == "群众" && lc.Sales == 1);
    }

    [Fact]
    public async Task GetGenderDataAsync_ReturnsCorrectGenderCounts()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // 添加测试数据
        var students = new List<StudentModel>
        {
            new() { UserId = "20123456", JoinTime = new DateTime(2020, 9, 1), Academy = "Computer", Gender = "男", PoliticalLandscape = "共青团员" },
            new() { UserId = "21123456", JoinTime = new DateTime(2021, 9, 1), Academy = "Computer", Gender = "男", PoliticalLandscape = "中共党员" },
            new() { UserId = "21234567", JoinTime = new DateTime(2021, 9, 1), Academy = "Information", Gender = "女", PoliticalLandscape = "共青团员" },
            new() { UserId = "22123456", JoinTime = new DateTime(2022, 9, 1), Academy = "Computer", Gender = "女", PoliticalLandscape = "共青团员" },
            new() { UserId = "22234567", JoinTime = new DateTime(2022, 9, 1), Academy = "Information", Gender = "男", PoliticalLandscape = "群众" },
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // Act
        var result = await _dataCentreService.GetGenderDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, gc => gc.Type == "男" && gc.Value == 3);
        Assert.Contains(result, gc => gc.Type == "女" && gc.Value == 2);
    }

    [Fact]
    public async Task GetYearDataAsync_WithEmptyDatabase_ReturnsDefaultYearData()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Act
        var result = await _dataCentreService.GetYearDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count >= 4); // 至少返回2019-2022学年的数据
    }

    [Fact]
    public async Task GetGradeDataAsync_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Act
        var result = await _dataCentreService.GetGradeDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}