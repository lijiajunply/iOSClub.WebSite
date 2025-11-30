using System.Data;
using System.Data.Common;
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
    public Task<Dictionary<string, IEnumerable<ArticleModel>>> GetAllCategoryArticles(string identity);
    public Task<bool> UpdateArticleOrders(Dictionary<string, int> articleOrders);

    public Task<IEnumerable<ArticleSearchResult>> SearchArticlesWithHighlights(string keyword, string identity = "",
        int pageSize = 20,
        int pageNumber = 1);
}

// 添加一个用于搜索结果的模型
[Serializable]
public class ArticleSearchResult : ArticleModel
{
    public string HighlightedTitle { get; set; } = "";
    public string HighlightedContent { get; set; } = "";
    public float Rank { get; set; }
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
            LastWriteTime = x.LastWriteTime,
            Category = x.Category == null ? null : new CategoryModel() { Name = x.Category.Name },
            Identity = x.Identity
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
            if (string.IsNullOrEmpty(model.Category?.Name))
            {
                model.CategoryId = null;
            }
            else
            {
                var category = await repository.GetByName(model.Category.Name);
                if (category == null)
                {
                    await repository.CreateOrUpdate(new CategoryModel() { Name = model.Category.Name });
                    category = await repository.GetByName(model.Category.Name);
                }

                model.CategoryId = category?.Id;
            }

            model.Category = null;

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

    public async Task<bool> UpdateArticleOrders(Dictionary<string, int>? articleOrders)
    {
        if (articleOrders == null || articleOrders.Count == 0)
        {
            return true;
        }

        await using var context = await factory.CreateDbContextAsync();

        // 开始事务
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // 获取所有需要更新的文章
            var articlePaths = articleOrders.Keys.ToList();
            var articles = await context.Articles
                .Where(a => articlePaths.Contains(a.Path))
                .ToListAsync();

            // 更新每篇文章的顺序
            foreach (var article in articles)
            {
                if (articleOrders.TryGetValue(article.Path, out var order))
                {
                    article.ArticleOrder = order;
                }
            }

            // 保存更改
            var result = await context.SaveChangesAsync();

            // 提交事务
            await transaction.CommitAsync();

            // 返回是否所有请求的文章都被更新
            return result > 0;
        }
        catch
        {
            // 回滚事务
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<ArticleSearchResult>> SearchArticlesWithHighlights(
        string keyword,
        string identity = "",
        int pageSize = 20,
        int pageNumber = 1)
    {
        await using var context = await factory.CreateDbContextAsync();

        const string sql = @"  
            WITH ranked_articles AS (  
                SELECT   
                    ""Path"", ""Title"", ""Content"", ""LastWriteTime"",   
                    ""Identity"", ""CategoryId"", ""ArticleOrder"",  
                    ts_headline('zhcfg', ""Title"", query) as highlighted_title,  
                    ts_headline('zhcfg', ""Content"", query, 'StartSel=<b>, StopSel=</b>, MaxWords=50, MinWords=15, ShortWord=3') as highlighted_content,  
        
                    -- 评分逻辑：如果 Title 直接 LIKE 命中，给极高的权重(比如 10)，否则用 standard rank
                    CASE 
                        WHEN ""Title"" LIKE '%' || @keyword || '%' THEN 10.0 
                        ELSE ts_rank(to_tsvector('zhcfg', coalesce(""Title"", '')), query) * 2 
                    END + 
                    ts_rank(to_tsvector('zhcfg', coalesce(""Content"", '')), query) as rank  

                FROM ""Articles"",  
                    plainto_tsquery('zhcfg', @keyword) as query  
    
                WHERE 
                    -- 条件 1：全文检索命中 (处理长文本、语义搜索)
                    (to_tsvector('zhcfg', coalesce(""Title"", '')) || to_tsvector('zhcfg', coalesce(""Content"", ''))) @@ query
                    OR 
                    -- 条件 2：标题简单模糊匹配 (处理停用词、短语、完全匹配)
                    ""Title"" LIKE '%' || @keyword || '%'
            )  
            SELECT * FROM ranked_articles  
            -- 过滤掉 rank 0 的情况（防止 query 为空时 LIKE 也未命中显示无关数据，视情况可加）
            WHERE rank > 0 
            ORDER BY rank DESC, ""LastWriteTime"" DESC   
            OFFSET @offset ROWS  
            FETCH NEXT @pageSize ROWS ONLY";

        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;

        AddParameter(command, "@keyword", keyword);
        AddParameter(command, "@offset", (pageNumber - 1) * pageSize);
        AddParameter(command, "@pageSize", pageSize);

        await context.Database.OpenConnectionAsync();
        await using var reader = await command.ExecuteReaderAsync();

        var results = new List<ArticleSearchResult>();
        if (string.IsNullOrEmpty(identity)) identity = "Member";
        while (await reader.ReadAsync())
        {
            var res = MapToArticleSearchResult(reader, identity);
            if (res != null) results.Add(res);
        }

        return results;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static ArticleSearchResult? MapToArticleSearchResult(DbDataReader reader, string identity)
    {
        var allowedIdentities = GetAllowedIdentities(identity);

        if (!allowedIdentities.Contains(reader.GetString("Identity")))
            return null;

        return new ArticleSearchResult
        {
            Path = reader.GetString("Path"),
            Title = reader.GetString("Title"),
            LastWriteTime = reader.GetDateTime("LastWriteTime"),
            Identity = reader.GetString("Identity"),
            HighlightedTitle = reader.GetString("highlighted_title"),
            HighlightedContent = reader.GetString("highlighted_content"),
            Rank = reader.GetFloat("rank")
        };
    }
}