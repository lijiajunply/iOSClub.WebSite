using iOSClub.Data.DataModels;
using iOSClub.DataApi.CQRS.Queries;
using iOSClub.DataApi.Repositories;
using iOSClub.DataApi.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace iOSClub.DataApi.CQRS.Handlers;

public class ArticleQueryHandler(IArticleRepository articleRepository, IDistributedCache distributedCache, IDataAccessStatisticsService statisticsService) :
    IQueryHandler<GetArticlesQuery, IEnumerable<ArticleModel>>,
    IQueryHandler<GetArticleByIdQuery, ArticleModel?>,
    IQueryHandler<GetArticlesByCategoryQuery, IEnumerable<ArticleModel>>
{
    private const string ArticlesCacheKey = "articles:all";
    private const string ArticleCacheKeyPrefix = "articles:";
    private const string ArticlesByCategoryCacheKeyPrefix = "articles:category:";
    private const int CacheExpirationMinutes = 30;

    public async Task<IEnumerable<ArticleModel>> Handle(GetArticlesQuery query, CancellationToken cancellationToken = default)
    {
        // 尝试从缓存获取
        var cachedArticles = await distributedCache.GetStringAsync(ArticlesCacheKey, cancellationToken);
        IEnumerable<ArticleModel> articles;
        
        if (!string.IsNullOrEmpty(cachedArticles))
        {
            articles = JsonConvert.DeserializeObject<IEnumerable<ArticleModel>>(cachedArticles)!;
        }
        else
        {
            // 缓存不存在，从数据库获取
            articles = await articleRepository.GetAll();

            // 存入缓存
            await distributedCache.SetStringAsync(
                ArticlesCacheKey,
                JsonConvert.SerializeObject(articles),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                cancellationToken);
        }
        
        // 记录访问统计
        await statisticsService.RecordDataAccessAsync("article", "all", "read", cancellationToken);

        return articles;
    }

    public async Task<ArticleModel?> Handle(GetArticleByIdQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ArticleCacheKeyPrefix}{query.Id}";
        
        // 尝试从缓存获取
        var cachedArticle = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        ArticleModel? article;
        
        if (!string.IsNullOrEmpty(cachedArticle))
        {
            article = JsonConvert.DeserializeObject<ArticleModel>(cachedArticle);
        }
        else
        {
            // 缓存不存在，从数据库获取
            article = await articleRepository.GetFromPath(query.Id);

            if (article != null)
            {
                // 存入缓存
                await distributedCache.SetStringAsync(
                    cacheKey,
                    JsonConvert.SerializeObject(article),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                    cancellationToken);
            }
        }
        
        // 记录访问统计
        await statisticsService.RecordDataAccessAsync("article", query.Id, "read", cancellationToken);

        return article;
    }

    public async Task<IEnumerable<ArticleModel>> Handle(GetArticlesByCategoryQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ArticlesByCategoryCacheKeyPrefix}{query.CategoryId}";
        
        // 尝试从缓存获取
        var cachedArticles = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        IEnumerable<ArticleModel> categoryArticles;
        
        if (!string.IsNullOrEmpty(cachedArticles))
        {
            categoryArticles = JsonConvert.DeserializeObject<IEnumerable<ArticleModel>>(cachedArticles)!;
        }
        else
        {
            // 缓存不存在，从数据库获取
            var articles = await articleRepository.GetAll();
            categoryArticles = articles.Where(a => a.Category?.Name == query.CategoryId || a.CategoryId == query.CategoryId);

            // 存入缓存
            await distributedCache.SetStringAsync(
                cacheKey,
                JsonConvert.SerializeObject(categoryArticles),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                cancellationToken);
        }
        
        // 记录访问统计
        await statisticsService.RecordDataAccessAsync("article", $"category:{query.CategoryId}", "read", cancellationToken);

        return categoryArticles;
    }
}