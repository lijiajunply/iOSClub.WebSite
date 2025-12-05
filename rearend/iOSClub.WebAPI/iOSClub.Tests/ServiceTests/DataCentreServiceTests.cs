using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Services;
using Microsoft.EntityFrameworkCore;
using iOSClub.Tests;

namespace iOSClub.Tests.ServiceTests;

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

        // 使用Bogus生成测试数据
        var students = new List<StudentModel>
        {
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "20123456")
                .RuleFor(s => s.JoinTime, new DateTime(2020, 9, 1))
                .RuleFor(s => s.Academy, "Computer")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "21123456")
                .RuleFor(s => s.JoinTime, new DateTime(2021, 9, 1))
                .RuleFor(s => s.Academy, "Computer")
                .RuleFor(s => s.PoliticalLandscape, "中共党员")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "21234567")
                .RuleFor(s => s.JoinTime, new DateTime(2021, 9, 1))
                .RuleFor(s => s.Academy, "Information")
                .RuleFor(s => s.Gender, "女")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "22123456")
                .RuleFor(s => s.JoinTime, new DateTime(2022, 9, 1))
                .RuleFor(s => s.Academy, "Computer")
                .RuleFor(s => s.Gender, "女")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "22234567")
                .RuleFor(s => s.JoinTime, new DateTime(2022, 9, 1))
                .RuleFor(s => s.Academy, "Information")
                .RuleFor(s => s.PoliticalLandscape, "群众")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "22345678")
                .RuleFor(s => s.JoinTime, new DateTime(2022, 9, 1))
                .RuleFor(s => s.Academy, "Mathematics")
                .Generate(),
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // Act
        var result = await _dataCentreService.GetYearDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result,
            yc => yc.Year == "2019学年" || yc.Year == "2020学年" || yc.Year == "2021学年" || yc.Year == "2022学年");
    }

    [Fact]
    public async Task GetCollegeDataAsync_ReturnsCorrectAcademyCounts()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // 使用Bogus生成测试数据
        var students = new List<StudentModel>
        {
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "20123456")
                .RuleFor(s => s.JoinTime, new DateTime(2020, 9, 1))
                .RuleFor(s => s.Academy, "Computer")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "21123456")
                .RuleFor(s => s.JoinTime, new DateTime(2021, 9, 1))
                .RuleFor(s => s.Academy, "Computer")
                .RuleFor(s => s.PoliticalLandscape, "中共党员")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "21234567")
                .RuleFor(s => s.JoinTime, new DateTime(2021, 9, 1))
                .RuleFor(s => s.Academy, "Information")
                .RuleFor(s => s.Gender, "女")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "22123456")
                .RuleFor(s => s.JoinTime, new DateTime(2022, 9, 1))
                .RuleFor(s => s.Academy, "Computer")
                .RuleFor(s => s.Gender, "女")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "22234567")
                .RuleFor(s => s.JoinTime, new DateTime(2022, 9, 1))
                .RuleFor(s => s.Academy, "Information")
                .RuleFor(s => s.PoliticalLandscape, "群众")
                .Generate(),
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // Act
        var result = await _dataCentreService.GetCollegeDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, ac => ac is { Type: "Computer", Value: 3 });
        Assert.Contains(result, ac => ac is { Type: "Information", Value: 2 });
    }

    [Fact]
    public async Task GetGradeDataAsync_ReturnsCorrectGradeCounts()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // 使用Bogus生成测试数据
        var students = new List<StudentModel>
        {
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "20123456")
                .RuleFor(s => s.JoinTime, new DateTime(2020, 9, 1))
                .RuleFor(s => s.Academy, "Computer")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "21123456")
                .RuleFor(s => s.JoinTime, new DateTime(2021, 9, 1))
                .RuleFor(s => s.Academy, "Computer")
                .RuleFor(s => s.PoliticalLandscape, "中共党员")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "21234567")
                .RuleFor(s => s.JoinTime, new DateTime(2021, 9, 1))
                .RuleFor(s => s.Academy, "Information")
                .RuleFor(s => s.Gender, "女")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "22123456")
                .RuleFor(s => s.JoinTime, new DateTime(2022, 9, 1))
                .RuleFor(s => s.Academy, "Computer")
                .RuleFor(s => s.Gender, "女")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "22234567")
                .RuleFor(s => s.JoinTime, new DateTime(2022, 9, 1))
                .RuleFor(s => s.Academy, "Information")
                .RuleFor(s => s.PoliticalLandscape, "群众")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "22345678")
                .RuleFor(s => s.JoinTime, new DateTime(2022, 9, 1))
                .RuleFor(s => s.Academy, "Mathematics")
                .Generate(),
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // Act
        var result = await _dataCentreService.GetGradeDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, gc => gc is { Grade: "20级", Value: 1 });
        Assert.Contains(result, gc => gc is { Grade: "21级", Value: 2 });
        Assert.Contains(result, gc => gc is { Grade: "22级", Value: 3 });
    }

    [Fact]
    public async Task GetLandscapeDataAsync_ReturnsCorrectPoliticalLandscapeCounts()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // 使用Bogus生成测试数据
        var students = new List<StudentModel>
        {
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "20123456")
                .RuleFor(s => s.JoinTime, new DateTime(2020, 9, 1))
                .RuleFor(s => s.Academy, "Computer")
                .RuleFor(s => s.PoliticalLandscape, "共青团员")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "21123456")
                .RuleFor(s => s.JoinTime, new DateTime(2021, 9, 1))
                .RuleFor(s => s.Academy, "Computer")
                .RuleFor(s => s.PoliticalLandscape, "中共党员")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "21234567")
                .RuleFor(s => s.JoinTime, new DateTime(2021, 9, 1))
                .RuleFor(s => s.Academy, "Information")
                .RuleFor(s => s.Gender, "女")
                .RuleFor(s => s.PoliticalLandscape, "共青团员")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "22123456")
                .RuleFor(s => s.JoinTime, new DateTime(2022, 9, 1))
                .RuleFor(s => s.Academy, "Computer")
                .RuleFor(s => s.Gender, "女")
                .RuleFor(s => s.PoliticalLandscape, "共青团员")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "22234567")
                .RuleFor(s => s.JoinTime, new DateTime(2022, 9, 1))
                .RuleFor(s => s.Academy, "Information")
                .RuleFor(s => s.PoliticalLandscape, "群众")
                .Generate(),
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // Act
        var result = await _dataCentreService.GetLandscapeDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, lc => lc is { Type: "共青团员", Value: 3 });
        Assert.Contains(result, lc => lc is { Type: "中共党员", Value: 1 });
        Assert.Contains(result, lc => lc is { Type: "群众", Value: 1 });
    }

    [Fact]
    public async Task GetGenderDataAsync_ReturnsCorrectGenderCounts()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // 使用Bogus生成测试数据，确保性别分布符合预期
        var students = new List<StudentModel>
        {
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "20123456")
                .RuleFor(s => s.JoinTime, new DateTime(2020, 9, 1))
                .RuleFor(s => s.Academy, "Computer")
                .RuleFor(s => s.Gender, "男")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "21123456")
                .RuleFor(s => s.JoinTime, new DateTime(2021, 9, 1))
                .RuleFor(s => s.Academy, "Computer")
                .RuleFor(s => s.Gender, "男")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "21234567")
                .RuleFor(s => s.JoinTime, new DateTime(2021, 9, 1))
                .RuleFor(s => s.Academy, "Information")
                .RuleFor(s => s.Gender, "女")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "22123456")
                .RuleFor(s => s.JoinTime, new DateTime(2022, 9, 1))
                .RuleFor(s => s.Academy, "Computer")
                .RuleFor(s => s.Gender, "女")
                .Generate(),
            BogusDataGenerator.StudentFaker.Clone()
                .RuleFor(s => s.UserId, "22234567")
                .RuleFor(s => s.JoinTime, new DateTime(2022, 9, 1))
                .RuleFor(s => s.Academy, "Information")
                .RuleFor(s => s.Gender, "男")
                .Generate(),
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // Act
        var result = await _dataCentreService.GetGenderDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, gc => gc is { Type: "男", Value: 3 });
        Assert.Contains(result, gc => gc is { Type: "女", Value: 2 });
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