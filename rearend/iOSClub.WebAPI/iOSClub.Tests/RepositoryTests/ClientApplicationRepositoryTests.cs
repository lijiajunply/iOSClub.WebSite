using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.Tests.RepositoryTests;

public class ClientApplicationRepositoryTests
{
    private readonly DbContextOptions<ClubContext> _options;
    private readonly ClientApplicationRepository _clientAppRepository;

    public ClientApplicationRepositoryTests()
    {
        // 使用内存数据库进行测试
        _options = new DbContextOptionsBuilder<ClubContext>()
            .UseInMemoryDatabase(databaseName: "ClientApplicationRepositoryTestDatabase")
            .Options;
        
        var contextFactory = new TestDbContextFactory(_options);
        _clientAppRepository = new ClientApplicationRepository(contextFactory);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllClientApplications()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var clientApps = new List<ClientApplication>
        {
            new() { ClientId = "app1", ClientSecret = "secret1", IsActive = true, RedirectUris = "http://localhost:3000" },
            new() { ClientId = "app2", ClientSecret = "secret2", IsActive = true, RedirectUris = "http://localhost:4000" }
        };
        await context.ClientApplications.AddRangeAsync(clientApps);
        await context.SaveChangesAsync();

        // Act
        var result = await _clientAppRepository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByClientIdAsync_ReturnsCorrectClientApplication()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var clientApp = new ClientApplication { ClientId = "app1", ClientSecret = "secret1", IsActive = true, RedirectUris = "http://localhost:3000" };
        await context.ClientApplications.AddAsync(clientApp);
        await context.SaveChangesAsync();

        // Act
        var result = await _clientAppRepository.GetByClientIdAsync("app1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(clientApp.ClientId, result.ClientId);
    }

    [Fact]
    public async Task CreateAsync_CreatesNewClientApplication()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var clientApp = new ClientApplication { ClientId = "app1", ClientSecret = "secret1", IsActive = true, RedirectUris = "http://localhost:3000" };

        // Act
        var result = await _clientAppRepository.CreateAsync(clientApp);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var savedClientApp = await newContext.ClientApplications.FirstOrDefaultAsync(c => c.ClientId == "app1");

        // Assert
        Assert.True(result);
        Assert.NotNull(savedClientApp);
        Assert.Equal(clientApp.ClientId, savedClientApp.ClientId);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingClientApplication()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var clientApp = new ClientApplication { ClientId = "app1", ClientSecret = "secret1", IsActive = true, RedirectUris = "http://localhost:3000" };
        await context.ClientApplications.AddAsync(clientApp);
        await context.SaveChangesAsync();
        
        // 修改客户端应用
        clientApp.IsActive = false;
        clientApp.RedirectUris = "http://localhost:3000;http://localhost:4000";

        // Act
        var result = await _clientAppRepository.UpdateAsync(clientApp);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var updatedClientApp = await newContext.ClientApplications.FirstOrDefaultAsync(c => c.ClientId == "app1");

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedClientApp);
        Assert.False(updatedClientApp.IsActive);
        Assert.Equal("http://localhost:3000;http://localhost:4000", updatedClientApp.RedirectUris);
    }

    [Fact]
    public async Task DeleteAsync_RemovesClientApplication()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var clientApp = new ClientApplication { ClientId = "app1", ClientSecret = "secret1", IsActive = true, RedirectUris = "http://localhost:3000" };
        await context.ClientApplications.AddAsync(clientApp);
        await context.SaveChangesAsync();

        // Act
        var result = await _clientAppRepository.DeleteAsync("app1");
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var deletedClientApp = await newContext.ClientApplications.FirstOrDefaultAsync(c => c.ClientId == "app1");

        // Assert
        Assert.True(result);
        Assert.Null(deletedClientApp);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_ReturnsClientApplication_WhenCredentialsAreValid()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var clientApp = new ClientApplication { ClientId = "app1", ClientSecret = "secret1", IsActive = true, RedirectUris = "http://localhost:3000" };
        await context.ClientApplications.AddAsync(clientApp);
        await context.SaveChangesAsync();

        // Act
        var result = await _clientAppRepository.ValidateCredentialsAsync("app1", "secret1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("app1", result.ClientId);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_ReturnsNull_WhenCredentialsAreInvalid()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var clientApp = new ClientApplication { ClientId = "app1", ClientSecret = "secret1", IsActive = true, RedirectUris = "http://localhost:3000" };
        await context.ClientApplications.AddAsync(clientApp);
        await context.SaveChangesAsync();

        // Act
        var result = await _clientAppRepository.ValidateCredentialsAsync("app1", "wrong-secret");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByRedirectUriAsync_ReturnsClientApplication_WhenRedirectUriExists()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var clientApp = new ClientApplication { ClientId = "app1", ClientSecret = "secret1", IsActive = true, RedirectUris = "http://localhost:3000;http://localhost:4000" };
        await context.ClientApplications.AddAsync(clientApp);
        await context.SaveChangesAsync();

        // Act
        var result = await _clientAppRepository.GetByRedirectUriAsync("http://localhost:3000");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("app1", result.ClientId);
    }
}
