using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.Tests.RepositoryTests;

public class ProjectRepositoryTests
{
    private readonly DbContextOptions<ClubContext> _options;
    private readonly ProjectRepository _projectRepository;
    private readonly TestDbContextFactory _contextFactory;

    public ProjectRepositoryTests()
    {
        // 使用内存数据库进行测试
        _options = new DbContextOptionsBuilder<ClubContext>()
            .UseInMemoryDatabase(databaseName: "ProjectRepositoryTestDatabase")
            .Options;
        
        _contextFactory = new TestDbContextFactory(_options);
        _projectRepository = new ProjectRepository(_contextFactory);
    }

    [Fact]
    public async Task GetAllProjectsAsync_ReturnsAllProjects()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成2个项目
        var projects = BogusDataGenerator.GenerateProjects(2);
        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();

        // Act
        var result = await _projectRepository.GetAllProjectsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetProjectByIdAsync_ReturnsCorrectProject()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个项目
        var project = BogusDataGenerator.ProjectFaker.Generate();
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await _projectRepository.GetProjectByIdAsync(project.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Id, result.Id);
        Assert.Equal(project.Title, result.Title);
    }

    [Fact]
    public async Task GetProjectByTitleAsync_ReturnsCorrectProject()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个项目
        var project = BogusDataGenerator.ProjectFaker.Generate();
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await _projectRepository.GetProjectByTitleAsync(project.Title);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Title, result.Title);
        Assert.Equal(project.Description, result.Description);
    }

    [Fact]
    public async Task ProjectExistsAsync_ReturnsTrue_WhenProjectExists()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个项目
        var project = BogusDataGenerator.ProjectFaker.Generate();
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await _projectRepository.ProjectExistsAsync(project.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ProjectExistsAsync_ReturnsFalse_WhenProjectDoesNotExist()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Act
        var result = await _projectRepository.ProjectExistsAsync("non-existent-project");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateProjectAsync_CreatesNewProject()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个员工
        var staff = BogusDataGenerator.StaffFaker.Generate();
        await context.Staffs.AddAsync(staff);
        await context.SaveChangesAsync();
        
        // 使用Bogus生成1个项目
        var project = BogusDataGenerator.ProjectFaker.Generate();
        project.Id = string.Empty; // 确保是新项目

        // Act
        var result = await _projectRepository.CreateProjectAsync(project, staff);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Title, result.Title);
        Assert.True(await _projectRepository.ProjectExistsAsync(result.Id));
    }

    [Fact]
    public async Task UpdateProjectAsync_UpdatesExistingProject()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个项目
        var project = BogusDataGenerator.ProjectFaker.Generate();
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();
        
        // 修改项目信息
        project.Title = "Updated Title";
        project.Description = "Updated Description";

        // Act
        var result = await _projectRepository.UpdateProjectAsync(project);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var updatedProject = await newContext.Projects.FirstOrDefaultAsync(p => p.Id == project.Id);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedProject);
        Assert.Equal("Updated Title", updatedProject.Title);
        Assert.Equal("Updated Description", updatedProject.Description);
    }

    [Fact]
    public async Task DeleteProjectAsync_RemovesProject()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个项目
        var project = BogusDataGenerator.ProjectFaker.Generate();
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await _projectRepository.DeleteProjectAsync(project.Id);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var deletedProject = await newContext.Projects.FirstOrDefaultAsync(p => p.Id == project.Id);

        // Assert
        Assert.True(result);
        Assert.Null(deletedProject);
    }

    [Fact]
    public async Task SearchProjectsAsync_ReturnsMatchingProjects()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成3个项目
        var projects = BogusDataGenerator.GenerateProjects(3);
        
        // 确保其中2个项目包含"Test"关键词
        projects[0].Title = "Test Project 1";
        projects[1].Description = "Test Description";
        projects[2].Title = "Project 3";
        
        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();

        // Act
        var result = await _projectRepository.SearchProjectsAsync("Test");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task HasProjectManagementPermissionAsync_ReturnsTrue_ForFounder()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个Founder身份的员工
        var staff = BogusDataGenerator.StaffFaker.Generate();
        staff.Identity = "Founder";
        await context.Staffs.AddAsync(staff);
        
        // 使用Bogus生成1个项目
        var project = BogusDataGenerator.ProjectFaker.Generate();
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await _projectRepository.HasProjectManagementPermissionAsync(staff.UserId, project.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasProjectManagementPermissionAsync_ReturnsTrue_ForPresident()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个President身份的员工
        var staff = BogusDataGenerator.StaffFaker.Generate();
        staff.Identity = "President";
        await context.Staffs.AddAsync(staff);
        
        // 使用Bogus生成1个项目
        var project = BogusDataGenerator.ProjectFaker.Generate();
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await _projectRepository.HasProjectManagementPermissionAsync(staff.UserId, project.Id);

        // Assert
        Assert.True(result);
    }
}
