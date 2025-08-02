using iOSClub.Data;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

public interface IArticleRepository
{
    public Task<IEnumerable<ArticleModel>> GetAll();
    public Task<ArticleModel?> GetFromPath(string path);
    public Task<bool> CreateOrUpdate(ArticleModel model);
    public Task<bool> Delete(string key);
}

public class ArticleRepository(IDbContextFactory<iOSContext> factory) : IArticleRepository
{
    public async Task<IEnumerable<ArticleModel>> GetAll()
    {
        await using var context = await factory.CreateDbContextAsync();
        var articles = await context.Articles.ToListAsync();
        return articles;
    }

    public async Task<ArticleModel?> GetFromPath(string path)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Articles.FirstOrDefaultAsync(x => x.Path == path);
    }

    public async Task<bool> CreateOrUpdate(ArticleModel model)
    {
        await using var context = await factory.CreateDbContextAsync();

        var article = await context.Articles.FirstOrDefaultAsync(x => x.Path == model.Path);
        if (article == null)
        {
            model.LastWriteTime = DateTime.Now;
            context.Articles.Add(model);
        }
        else
        {
            article.Content = model.Content;
            article.LastWriteTime = DateTime.Now;
            article.Title = model.Title;
            article.Identity = model.Identity;
            article.Path = model.Path;
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
}