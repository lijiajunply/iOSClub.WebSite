using iOSClub.Data.DataModels;
using iOSClub.DataApi.CQRS.Queries;
using iOSClub.DataApi.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace iOSClub.DataApi.CQRS.Handlers;

public class ArticleQueryHandler(IArticleRepository articleRepository, IDistributedCache distributedCache) :
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
        if (!string.IsNullOrEmpty(cachedArticles))
        {
            return JsonConvert.DeserializeObject<IEnumerable<ArticleModel>>(cachedArticles)!;
        }

        // 缓存不存在，从数据库获取
        var articles = await articleRepository.GetAll();

        // 存入缓存
        await distributedCache.SetStringAsync(
            ArticlesCacheKey,
            JsonConvert.SerializeObject(articles),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
            cancellationToken);

        return articles;
    }

    public async Task<ArticleModel?> Handle(GetArticleByIdQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ArticleCacheKeyPrefix}{query.Id}";
        
        // 尝试从缓存获取
        var cachedArticle = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedArticle))
        {
            return JsonConvert.DeserializeObject<ArticleModel>(cachedArticle);
        }

        // 缓存不存在，从数据库获取
        var article = await articleRepository.GetFromPath(query.Id);

        if (article != null)
        {
            // 存入缓存
            await distributedCache.SetStringAsync(
                cacheKey,
                JsonConvert.SerializeObject(article),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                cancellationToken);
        }

        return article;
    }

    public async Task<IEnumerable<ArticleModel>> Handle(GetArticlesByCategoryQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ArticlesByCategoryCacheKeyPrefix}{query.CategoryId}";
        
        // 尝试从缓存获取
        var cachedArticles = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedArticles))
        {
            return JsonConvert.DeserializeObject<IEnumerable<ArticleModel>>(cachedArticles)!;
        }

        // 缓存不存在，从数据库获取
        var articles = await articleRepository.GetAll();
        var categoryArticles = articles.Where(a => a.Category?.Name == query.CategoryId || a.CategoryId == query.CategoryId);

        // 存入缓存
        await distributedCache.SetStringAsync(
            cacheKey,
            JsonConvert.SerializeObject(categoryArticles),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
            cancellationToken);

        return categoryArticles;
    }
}