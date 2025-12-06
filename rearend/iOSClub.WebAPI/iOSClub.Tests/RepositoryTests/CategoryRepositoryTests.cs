using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.Tests.RepositoryTests;

public class CategoryRepositoryTests
{
    private readonly DbContextOptions<ClubContext> _options;
    private readonly CategoryRepository _categoryRepository;

    public CategoryRepositoryTests()
    {
        // 使用内存数据库进行测试
        _options = new DbContextOptionsBuilder<ClubContext>()
            .UseInMemoryDatabase(databaseName: "CategoryRepositoryTestDatabase")
            .Options;
        
        var contextFactory = new TestDbContextFactory(_options);
        _categoryRepository = new CategoryRepository(contextFactory);
    }

    [Fact]
    public async Task GetAll_ReturnsAllCategories()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成2个分类
        var categories = BogusDataGenerator.GenerateCategories(2);
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // Act
        var result = await _categoryRepository.GetAll();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByName_ReturnsCorrectCategory()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个分类
        var category = BogusDataGenerator.CategoryFaker.Generate();
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        // Act
        var result = await _categoryRepository.GetByName(category.Name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Name, result.Name);
    }

    [Fact]
    public async Task CreateOrUpdate_CreatesNewCategory()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个分类
        var category = BogusDataGenerator.CategoryFaker.Generate();
        category.Id = string.Empty; // 确保是新分类

        // Act
        var result = await _categoryRepository.CreateOrUpdate(category);
        var savedCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == category.Name);

        // Assert
        Assert.True(result);
        Assert.NotNull(savedCategory);
        Assert.Equal(category.Name, savedCategory.Name);
        Assert.Equal(category.Description, savedCategory.Description);
    }

    [Fact]
    public async Task CreateOrUpdate_UpdatesExistingCategory()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个分类
        var category = BogusDataGenerator.CategoryFaker.Generate();
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();
        
        // 更新分类信息
        category.Description = "Updated Description";
        category.Order = 2;

        // Act
        var result = await _categoryRepository.CreateOrUpdate(category);
        var updatedCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == category.Name);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedCategory);
        Assert.Equal("Updated Description", updatedCategory.Description);
        Assert.Equal(2, updatedCategory.Order);
    }

    [Fact]
    public async Task Delete_RemovesCategory()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个分类
        var category = BogusDataGenerator.CategoryFaker.Generate();
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        // Act
        var result = await _categoryRepository.Delete(category.Name);
        var deletedCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == category.Name);

        // Assert
        Assert.True(result);
        Assert.Null(deletedCategory);
    }

    [Fact]
    public async Task UpdateCategoryOrder_UpdatesCategoryOrder()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个分类
        var category = BogusDataGenerator.CategoryFaker.Generate();
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        // Act
        var result = await _categoryRepository.UpdateCategoryOrder(category.Name, 3);
        
        // 创建新的context实例来查询更新后的结果
        await using var newContext = new ClubContext(_options);
        var updatedCategory = await newContext.Categories.FirstOrDefaultAsync(c => c.Name == category.Name);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedCategory);
        Assert.Equal(3, updatedCategory.Order);
    }

    [Fact]
    public async Task GetById_ReturnsCorrectCategory()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个分类
        var category = BogusDataGenerator.CategoryFaker.Generate();
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        // Act
        var result = await _categoryRepository.GetById(category.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
    }

    [Fact]
    public async Task GetArticlesById_ReturnsCorrectArticles()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成1个分类
        var category = BogusDataGenerator.CategoryFaker.Generate();
        await context.Categories.AddAsync(category);
        
        // 使用Bogus生成2篇文章
        var articles = BogusDataGenerator.GenerateArticles(2);
        foreach (var article in articles)
        {
            article.CategoryId = category.Id;
        }
        await context.Articles.AddRangeAsync(articles);
        await context.SaveChangesAsync();

        // Act
        var result = await _categoryRepository.GetArticlesById(category.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
    }
}
