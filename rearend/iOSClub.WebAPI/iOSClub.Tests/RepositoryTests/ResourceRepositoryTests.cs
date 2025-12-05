using iOSClub.Data;
using iOSClub.DataApi.Repositories;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.Tests;

public class ResourceRepositoryTests
{
    private readonly DbContextOptions<ClubContext> _options;
    private readonly ClubContext _context;
    private readonly ResourceRepository _resourceRepository;

    public ResourceRepositoryTests()
    {
        // 使用内存数据库进行测试
        _options = new DbContextOptionsBuilder<ClubContext>()
            .UseInMemoryDatabase(databaseName: "ResourceRepositoryTestDatabase")
            .Options;

        _context = new ClubContext(_options);
        _resourceRepository = new ResourceRepository(_context);
    }

    [Fact]
    public async Task GetAllResourcesAsync_ReturnsAllResources()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        var resources = new List<ResourceModel>
        {
            new() { Id = "res1", Name = "Resource 1", Tag = "tag1" },
            new() { Id = "res2", Name = "Resource 2", Tag = "tag2" }
        };
        await _context.Resources.AddRangeAsync(resources);
        await _context.SaveChangesAsync();

        // Act
        var result = await _resourceRepository.GetAllResourcesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetResourceByIdAsync_ReturnsCorrectResource()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        var resource = new ResourceModel { Id = "res1", Name = "Test Resource", Tag = "test-tag" };
        await _context.Resources.AddAsync(resource);
        await _context.SaveChangesAsync();

        // Act
        var result = await _resourceRepository.GetResourceByIdAsync("res1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(resource.Id, result.Id);
        Assert.Equal(resource.Name, result.Name);
    }

    [Fact]
    public async Task GetResourcesByTagAsync_ReturnsResourcesWithMatchingTag()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        var resources = new List<ResourceModel>
        {
            new() { Id = "res1", Name = "Resource 1", Tag = "tag1" },
            new() { Id = "res2", Name = "Resource 2", Tag = "tag2" },
            new() { Id = "res3", Name = "Resource 3", Tag = "tag1" }
        };
        await _context.Resources.AddRangeAsync(resources);
        await _context.SaveChangesAsync();

        // Act
        var result = await _resourceRepository.GetResourcesByTagAsync("tag1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal("tag1", r.Tag));
    }

    [Fact]
    public async Task SearchResourcesByNameAsync_ReturnsResourcesWithMatchingName()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        var resources = new List<ResourceModel>
        {
            new() { Id = "res1", Name = "Test Resource 1", Tag = "tag1" },
            new() { Id = "res2", Name = "Another Resource", Tag = "tag2" },
            new() { Id = "res3", Name = "Resource 3", Tag = "tag1" }
        };
        await _context.Resources.AddRangeAsync(resources);
        await _context.SaveChangesAsync();

        // Act
        var result = await _resourceRepository.SearchResourcesByNameAsync("Test");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test Resource 1", result[0].Name);
    }

    [Fact]
    public async Task AddResourceAsync_AddsNewResource()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        var resource = new ResourceModel { Name = "New Resource", Tag = "new-tag" };

        // Act
        var result = await _resourceRepository.AddResourceAsync(resource);

        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var savedResource = await newContext.Resources.FirstOrDefaultAsync(r => r.Name == "New Resource");

        // Assert
        Assert.True(result);
        Assert.NotNull(savedResource);
        Assert.Equal(resource.Name, savedResource.Name);
        Assert.NotNull(savedResource.Id);
    }

    [Fact]
    public async Task UpdateResourceAsync_UpdatesExistingResource()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        var resource = new ResourceModel { Id = "res1", Name = "Original Name", Tag = "original-tag" };
        await _context.Resources.AddAsync(resource);
        await _context.SaveChangesAsync();

        // 修改资源信息
        resource.Name = "Updated Name";
        resource.Tag = "updated-tag";

        // Act
        var result = await _resourceRepository.UpdateResourceAsync(resource);

        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var updatedResource = await newContext.Resources.FirstOrDefaultAsync(r => r.Id == "res1");

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedResource);
        Assert.Equal("Updated Name", updatedResource.Name);
        Assert.Equal("updated-tag", updatedResource.Tag);
    }

    [Fact]
    public async Task DeleteResourceAsync_RemovesResource()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        var resource = new ResourceModel { Id = "res1", Name = "Test Resource", Tag = "test-tag" };
        await _context.Resources.AddAsync(resource);
        await _context.SaveChangesAsync();

        // Act
        var result = await _resourceRepository.DeleteResourceAsync("res1");

        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var deletedResource = await newContext.Resources.FirstOrDefaultAsync(r => r.Id == "res1");

        // Assert
        Assert.True(result);
        Assert.Null(deletedResource);
    }

    [Fact]
    public async Task GetAllTagsAsync_ReturnsAllUniqueTags()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        var resources = new List<ResourceModel>
        {
            new() { Id = "res1", Name = "Resource 1", Tag = "tag1" },
            new() { Id = "res2", Name = "Resource 2", Tag = "tag2" },
            new() { Id = "res3", Name = "Resource 3", Tag = "tag1" }
        };
        await _context.Resources.AddRangeAsync(resources);
        await _context.SaveChangesAsync();

        // Act
        var result = await _resourceRepository.GetAllTagsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("tag1", result);
        Assert.Contains("tag2", result);
    }

    [Fact]
    public async Task GetResourceCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        var resources = new List<ResourceModel>
        {
            new() { Id = "res1", Name = "Resource 1", Tag = "tag1" },
            new() { Id = "res2", Name = "Resource 2", Tag = "tag2" },
            new() { Id = "res3", Name = "Resource 3", Tag = "tag3" }
        };
        await _context.Resources.AddRangeAsync(resources);
        await _context.SaveChangesAsync();

        // Act
        var result = await _resourceRepository.GetResourceCountAsync();

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public async Task ResourceExistsAsync_ReturnsTrue_WhenResourceExists()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        var resource = new ResourceModel { Id = "res1", Name = "Test Resource", Tag = "test-tag" };
        await _context.Resources.AddAsync(resource);
        await _context.SaveChangesAsync();

        // Act
        var result = await _resourceRepository.ResourceExistsAsync("res1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ResourceExistsAsync_ReturnsFalse_WhenResourceDoesNotExist()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        // Act
        var result = await _resourceRepository.ResourceExistsAsync("non-existent-resource");

        // Assert
        Assert.False(result);
    }
}