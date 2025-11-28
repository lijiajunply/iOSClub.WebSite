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

public class ArticleRepository(IDbContextFactory<ClubContext> factory) : IArticleRepository
{
    public async Task<IEnumerable<ArticleModel>> GetAll()
    {
        await using var context = await factory.CreateDbContextAsync();
        var articles = await context.Articles.ToListAsync();
        return articles;
    }

    public async Task<ArticleModel?> GetFromPath(string path, string identity = "")
    {
        await using var context = await factory.CreateDbContextAsync();
        var article = await context.Articles.FirstOrDefaultAsync(x =>
            x.Path == path);

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
            article.Category = model.Category;
        }

        return await context.SaveChangesAsync() == 1;
    }

    public async Task<bool> Delete(string key)
    {
        await using var context = await factory.CreateDbContextAsync();
        var article = await context.Articles.FirstOrDefaultAsync(x => x.Path == key);

        if (article == null) return false;
        context.Articles.Remove(article);
        return await context.SaveChangesAsync() == 1;
    }

    /// <summary>
    /// 获取分类下的文章
    /// </summary>
    /// <param name="category">分类名</param>
    /// <returns></returns>
    public async Task<IEnumerable<ArticleModel>> GetCategoryArticles(string category)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Articles.Where(x => x.Category == category).ToListAsync();
    }

    /// <summary>
    /// 获取所有分类下的文章
    /// </summary>
    /// <returns></returns>
    public async Task<Dictionary<string, IEnumerable<ArticleModel>>> GetAllCategoryArticles(string identity)
    {
        await using var context = await factory.CreateDbContextAsync();
        var articles = await context.Articles
            .Select(x => new ArticleModel
            {
                Path = x.Path,
                Title = x.Title,
                Identity = x.Identity,
                Category = x.Category ?? "其他"
            })
            .ToListAsync();

        return articles
            .Where(x => IsIdentityExist(identity, x.Identity))
            .GroupBy(x => x.Category!)
            .ToDictionary(x => x.Key, x => x.AsEnumerable());
    }


    /// <summary>
    /// 判断用户是否满足需要的身份等级
    /// </summary>
    /// <param name="identity">用户的身份等级</param>
    /// <param name="neededIdentity">需要的身份等级</param>
    /// <returns></returns>
    private static bool IsIdentityExist(string identity, string? neededIdentity)
    {
        if (string.IsNullOrEmpty(neededIdentity) || (identity == "" && neededIdentity == "Member")) return true;

        string[] identities = ["Founder", "President", "Minister", "Department", "Member", ""];
        if (!identities.Contains(identity) || !identities.Contains(neededIdentity)) return false;
        var identityNum = Array.IndexOf(identities, identity);
        var neededIdentityNum = Array.IndexOf(identities, neededIdentity);
        return identityNum >= neededIdentityNum;
    }
}