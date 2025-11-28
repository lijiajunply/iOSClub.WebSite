using iOSClub.Data;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

public interface IArticleRepository
{
    public Task<IEnumerable<ArticleModel>> GetAll();
    public Task<ArticleModel?> GetFromPath(string path, string identity = "");
    public Task<bool> CreateOrUpdate(ArticleModel model);
    public Task<bool> Delete(string key);
    public Task<IEnumerable<ArticleModel>> GetCategoryArticles(string category);
    public Task<Dictionary<string, IEnumerable<ArticleModel>>> GetAllCategoryArticles(string identity);
}

public class ArticleRepository(IDbContextFactory<ClubContext> factory, ICategoryRepository repository)
    : IArticleRepository
{
    public async Task<IEnumerable<ArticleModel>> GetAll()
    {
        await using var context = await factory.CreateDbContextAsync();
        var articles = await context.Articles.Include(x => x.Category).Select(x => new ArticleModel()
        {
            Path = x.Path,
            Title = x.Title,
            Content = x.Content,
            LastWriteTime = x.LastWriteTime,
            Category = x.Category == null ? null : new CategoryModel() { Name = x.Category.Name },
            ArticleOrder = x.ArticleOrder,
            Identity = x.Identity,
            CategoryId = x.CategoryId,
        }).ToListAsync();
        return articles;
    }

    public async Task<ArticleModel?> GetFromPath(string path, string identity = "")
    {
        await using var context = await factory.CreateDbContextAsync();
        var article = await context.Articles.Include(x => x.Category)
            .Select(x => new ArticleModel()
            {
                Path = x.Path,
                Title = x.Title,
                Content = x.Content,
                LastWriteTime = x.LastWriteTime,
                Category = x.Category == null ? null : new CategoryModel() { Name = x.Category.Name },
                ArticleOrder = x.ArticleOrder,
                Identity = x.Identity,
                CategoryId = x.CategoryId,
            })
            .FirstOrDefaultAsync(x => x.Path == path);

        if (article == null)
            return null;

        if (string.IsNullOrEmpty(identity) || IsIdentityExist(identity, article.Identity))
            return article;

        return null;
    }

    public async Task<bool> CreateOrUpdate(ArticleModel model)
    {
        await using var context = await factory.CreateDbContextAsync();

        var article = await context.Articles.FirstOrDefaultAsync(x => x.Path == model.Path);
        if (article == null)
        {
            model.LastWriteTime = DateTime.UtcNow;
            context.Articles.Add(model);
        }
        else
        {
            article.Content = model.Content;
            article.LastWriteTime = DateTime.UtcNow;
            article.Title = model.Title;
            article.Identity = model.Identity;

            if (string.IsNullOrEmpty(model.Category?.Name))
            {
                article.CategoryId = null;
            }
            else
            {
                var category = await repository.GetByName(model.Category.Name);
                if (category == null)
                {
                    await repository.CreateOrUpdate(new CategoryModel() { Name = model.Category.Name });
                    category = await repository.GetByName(model.Category.Name);
                }

                article.CategoryId = category?.Id;
            }
        }

        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Delete(string key)
    {
        await using var context = await factory.CreateDbContextAsync();
        var article = await context.Articles.FirstOrDefaultAsync(x => x.Path == key);

        if (article == null) return false;
        context.Articles.Remove(article);
        return await context.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// 获取分类下的文章
    /// </summary>
    /// <param name="category">分类名</param>
    /// <returns></returns>
    public async Task<IEnumerable<ArticleModel>> GetCategoryArticles(string category)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Articles.Include(x => x.Category)
            .Where(x => x.Category != null)
            .Where(x => x.Category!.Name == category).ToListAsync();
    }

    private static readonly Dictionary<string, int> IdentityLevels = new()
    {
        ["Founder"] = 0, ["President"] = 1, ["Minister"] = 2,
        ["Department"] = 3, ["Member"] = 4, [""] = 5
    };

    public async Task<Dictionary<string, IEnumerable<ArticleModel>>> GetAllCategoryArticles(string identity)
    {
        await using var context = await factory.CreateDbContextAsync();

        if (string.IsNullOrEmpty(identity)) identity = "Member";

        var allowedIdentities = GetAllowedIdentities(identity);

        var query = context.Articles
            .AsNoTracking()
            .Where(a => a.Identity == null || ((IEnumerable<string>)allowedIdentities).Contains(a.Identity))
            .Select(a => new ArticleProjection
            {
                Path = a.Path,
                Title = a.Title,
                ArticleOrder = a.ArticleOrder,
                LastWriteTime = a.LastWriteTime,
                CategoryName = a.Category != null ? a.Category.Name : "其他",
                CategoryOrder = a.Category != null ? a.Category.Order : int.MaxValue,
                Identity = a.Identity
            })
            .OrderBy(a => a.CategoryOrder)
            .ThenBy(a => a.ArticleOrder);

        var articles = await query.ToListAsync();

        return articles
            .GroupBy(article => article.CategoryName)
            .ToDictionary(group => group.Key, group => group.Select(x => new ArticleModel()
            {
                Path = x.Path,
                Title = x.Title,
                Identity = x.Identity,
                LastWriteTime = x.LastWriteTime,
                Category = new CategoryModel()
                {
                    Name = x.CategoryName
                }
            }).AsEnumerable());
    }

    private static bool IsIdentityExist(string identity, string? neededIdentity)
    {
        if (string.IsNullOrEmpty(neededIdentity) ||
            (identity == "" && neededIdentity == "Member"))
            return true;

        return IdentityLevels.TryGetValue(identity, out var identityLevel) &&
               IdentityLevels.TryGetValue(neededIdentity, out var neededLevel) &&
               identityLevel <= neededLevel;
    }

    private static string[] GetAllowedIdentities(string identity)
    {
        if (!IdentityLevels.TryGetValue(identity, out var level))
            return [""];

        return IdentityLevels
            .Where(kvp => kvp.Value >= level)
            .Select(kvp => kvp.Key)
            .ToArray();
    }

    [Serializable]
    private class ArticleProjection
    {
        public string Path { get; set; } = "";
        public string Title { get; set; } = "";
        public DateTime LastWriteTime { get; set; } = DateTime.UtcNow;
        public int ArticleOrder { get; set; }
        public string CategoryName { get; set; } = "";
        public int CategoryOrder { get; set; }
        public string? Identity { get; set; }
    }
}