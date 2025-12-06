using iOSClub.Data.DataModels;
using iOSClub.DataApi.CQRS.Commands;
using iOSClub.DataApi.CQRS.Queries;
using iOSClub.DataApi.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace iOSClub.DataApi.CQRS.Handlers;

public class ArticleQueryHandler(IArticleRepository articleRepository) : 
    IQueryHandler<GetArticlesQuery, IEnumerable<ArticleModel>>,
    IQueryHandler<GetArticleByIdQuery, ArticleModel?>,
    IQueryHandler<GetArticlesByCategoryQuery, IEnumerable<ArticleModel>>
{
    private const string ArticlesCacheKey = "articles:all";
    private const string ArticleCacheKeyPrefix = "articles:";
    private const int CacheExpirationMinutes = 30;

    public async Task<IEnumerable<ArticleModel>> Handle(GetArticlesQuery query, CancellationToken cancellationToken = default)
    {
        // 从缓存获取或从数据库查询
        var articles = await articleRepository.GetAll();
        return articles.Cast<ArticleModel>();
    }

    public async Task<ArticleModel?> Handle(GetArticleByIdQuery query, CancellationToken cancellationToken = default)
    {
        // 从数据库查询指定ID的文章
        var article = await articleRepository.GetFromPath(query.Id);
        return article;
    }

    public async Task<IEnumerable<ArticleModel>> Handle(GetArticlesByCategoryQuery query, CancellationToken cancellationToken = default)
    {
        // 从数据库查询指定分类的文章
        var articles = await articleRepository.GetAll();
        return articles.Where(a => a.Category?.Name == query.CategoryId || a.CategoryId == query.CategoryId);
    }
}