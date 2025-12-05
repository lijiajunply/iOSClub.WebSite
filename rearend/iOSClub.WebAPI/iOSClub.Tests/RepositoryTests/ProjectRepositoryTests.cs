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
        
        var projects = new List<ProjectModel>
        {
            new() { Id = "project1", Title = "Project 1", Description = "Description 1" },
            new() { Id = "project2", Title = "Project 2", Description = "Description 2" }
        };
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
        
        var project = new ProjectModel { Id = "project1", Title = "Test Project", Description = "Test Description" };
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await _projectRepository.GetProjectByIdAsync("project1");

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
        
        var project = new ProjectModel { Id = "project1", Title = "Test Project", Description = "Test Description" };
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await _projectRepository.GetProjectByTitleAsync("Test Project");

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
        
        var project = new ProjectModel { Id = "project1", Title = "Test Project", Description = "Test Description" };
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await _projectRepository.ProjectExistsAsync("project1");

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
        
        var staff = new StaffModel { UserId = "staff1", Name = "Staff 1", Identity = "Member" };
        await context.Staffs.AddAsync(staff);
        await context.SaveChangesAsync();
        
        var project = new ProjectModel { Title = "New Project", Description = "New Description" };

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
        
        var project = new ProjectModel { Id = "project1", Title = "Original Title", Description = "Original Description" };
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();
        
        // 修改项目信息
        project.Title = "Updated Title";
        project.Description = "Updated Description";

        // Act
        var result = await _projectRepository.UpdateProjectAsync(project);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var updatedProject = await newContext.Projects.FirstOrDefaultAsync(p => p.Id == "project1");

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
        
        var project = new ProjectModel { Id = "project1", Title = "Test Project", Description = "Test Description" };
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await _projectRepository.DeleteProjectAsync("project1");
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var deletedProject = await newContext.Projects.FirstOrDefaultAsync(p => p.Id == "project1");

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
        
        var projects = new List<ProjectModel>
        {
            new() { Id = "project1", Title = "Test Project 1", Description = "Description 1" },
            new() { Id = "project2", Title = "Another Project", Description = "Test Description" },
            new() { Id = "project3", Title = "Project 3", Description = "Description 3" }
        };
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
        
        var staff = new StaffModel { UserId = "staff1", Name = "Staff 1", Identity = "Founder" };
        await context.Staffs.AddAsync(staff);
        
        var project = new ProjectModel { Id = "project1", Title = "Test Project", Description = "Test Description" };
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await _projectRepository.HasProjectManagementPermissionAsync("staff1", "project1");

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
        
        var staff = new StaffModel { UserId = "staff1", Name = "Staff 1", Identity = "President" };
        await context.Staffs.AddAsync(staff);
        
        var project = new ProjectModel { Id = "project1", Title = "Test Project", Description = "Test Description" };
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await _projectRepository.HasProjectManagementPermissionAsync("staff1", "project1");

        // Assert
        Assert.True(result);
    }
}
