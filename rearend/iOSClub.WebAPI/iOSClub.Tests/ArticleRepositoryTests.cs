using iOSClub.Data;
using iOSClub.DataApi.Repositories;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.Tests;

public class ArticleRepositoryTests
{
    private readonly DbContextOptions<ClubContext> _options;
    private readonly ArticleRepository _articleRepository;
    private readonly CategoryRepository _categoryRepository;

    public ArticleRepositoryTests()
    {
        // 使用内存数据库进行测试
        _options = new DbContextOptionsBuilder<ClubContext>()
            .UseInMemoryDatabase(databaseName: "ArticleRepositoryTestDatabase")
            .Options;
        
        var contextFactory = new TestDbContextFactory(_options);
        _categoryRepository = new CategoryRepository(contextFactory);
        _articleRepository = new ArticleRepository(contextFactory, _categoryRepository);
    }

    [Fact]
    public async Task GetAll_ReturnsAllArticles()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var category = new CategoryModel { Id = "test-category", Name = "Test Category", Order = 1 };
        await context.Categories.AddAsync(category);
        
        var articles = new List<ArticleModel>
        {
            new() { Path = "/test/article1", Title = "Article 1", Content = "Content 1", CategoryId = category.Id, Identity = "Member" },
            new() { Path = "/test/article2", Title = "Article 2", Content = "Content 2", CategoryId = category.Id, Identity = "Member" }
        };
        await context.Articles.AddRangeAsync(articles);
        await context.SaveChangesAsync();

        // Act
        var result = await _articleRepository.GetAll();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetFromPath_ReturnsCorrectArticle()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var article = new ArticleModel { Path = "/test/article", Title = "Test Article", Content = "Test Content", Identity = "Member" };
        await context.Articles.AddAsync(article);
        await context.SaveChangesAsync();

        // Act
        var result = await _articleRepository.GetFromPath("/test/article");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(article.Title, result.Title);
        Assert.Equal(article.Content, result.Content);
    }

    [Fact]
    public async Task CreateOrUpdate_CreatesNewArticle()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var article = new ArticleModel { Path = "/test/newarticle", Title = "New Article", Content = "New Content", Identity = "Member" };

        // Act
        var result = await _articleRepository.CreateOrUpdate(article);
        var savedArticle = await context.Articles.FirstOrDefaultAsync(a => a.Path == article.Path);

        // Assert
        Assert.True(result);
        Assert.NotNull(savedArticle);
        Assert.Equal(article.Title, savedArticle.Title);
    }

    [Fact]
    public async Task CreateOrUpdate_UpdatesExistingArticle()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var article = new ArticleModel { Path = "/test/article", Title = "Original Title", Content = "Original Content", Identity = "Member" };
        await context.Articles.AddAsync(article);
        await context.SaveChangesAsync();
        
        article.Title = "Updated Title";
        article.Content = "Updated Content";

        // Act
        var result = await _articleRepository.CreateOrUpdate(article);
        var updatedArticle = await context.Articles.FirstOrDefaultAsync(a => a.Path == article.Path);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedArticle);
        Assert.Equal("Updated Title", updatedArticle.Title);
        Assert.Equal("Updated Content", updatedArticle.Content);
    }

    [Fact]
    public async Task Delete_RemovesArticle()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var article = new ArticleModel { Path = "/test/article", Title = "Test Article", Content = "Test Content", Identity = "Member" };
        await context.Articles.AddAsync(article);
        await context.SaveChangesAsync();

        // Act
        var result = await _articleRepository.Delete("/test/article");
        var deletedArticle = await context.Articles.FirstOrDefaultAsync(a => a.Path == "/test/article");

        // Assert
        Assert.True(result);
        Assert.Null(deletedArticle);
    }

    [Fact]
    public async Task GetAllCategoryArticles_ReturnsArticlesGroupedByCategory()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var category1 = new CategoryModel { Id = "cat1", Name = "Category 1", Order = 1 };
        var category2 = new CategoryModel { Id = "cat2", Name = "Category 2", Order = 2 };
        await context.Categories.AddRangeAsync(category1, category2);
        
        var articles = new List<ArticleModel>
        {
            new() { Path = "/test/article1", Title = "Article 1", Content = "Content 1", CategoryId = category1.Id, Identity = "Member" },
            new() { Path = "/test/article2", Title = "Article 2", Content = "Content 2", CategoryId = category1.Id, Identity = "Member" },
            new() { Path = "/test/article3", Title = "Article 3", Content = "Content 3", CategoryId = category2.Id, Identity = "Member" }
        };
        await context.Articles.AddRangeAsync(articles);
        await context.SaveChangesAsync();

        // Act
        var result = await _articleRepository.GetAllCategoryArticles("Member");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("Category 1"));
        Assert.True(result.ContainsKey("Category 2"));
        Assert.Equal(2, result["Category 1"].Count());
        Assert.Single(result["Category 2"]);
    }
}
