using iOSClub.Data.DataModels;
using iOSClub.DataApi.CQRS.Commands;
using iOSClub.DataApi.Repositories;
using Microsoft.Extensions.Caching.Distributed;

namespace iOSClub.DataApi.CQRS.Handlers;

public class ArticleCommandHandler(IArticleRepository articleRepository, IDistributedCache distributedCache) : 
    ICommandHandler<CreateArticleCommand, bool>,
    ICommandHandler<UpdateArticleCommand, bool>,
    ICommandHandler<DeleteArticleCommand, bool>,
    ICommandHandler<UpdateManyArticlesCommand, bool>
{
    private const string ArticlesCacheKey = "articles:all";
    private const string ArticleCacheKeyPrefix = "articles:";
    private const string ArticlesByCategoryCacheKeyPrefix = "articles:category:";

    public async Task<bool> Handle(CreateArticleCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法创建或更新文章
        var result = await articleRepository.CreateOrUpdate(command.Article);
        
        if (result)
        {
            // 清除相关缓存
            await ClearArticleCache(command.Article.Path, command.Article.CategoryId, cancellationToken);
        }
        
        return result;
    }

    public async Task<bool> Handle(UpdateArticleCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法更新文章
        var result = await articleRepository.CreateOrUpdate(command.Article);
        
        if (result)
        {
            // 清除相关缓存
            await ClearArticleCache(command.Article.Path, command.Article.CategoryId, cancellationToken);
        }
        
        return result;
    }

    public async Task<bool> Handle(DeleteArticleCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法删除文章
        var result = await articleRepository.Delete(command.Id);
        
        if (result)
        {
            // 清除相关缓存
            await ClearArticleCache(command.Id, null, cancellationToken);
        }
        
        return result;
    }

    public async Task<bool> Handle(UpdateManyArticlesCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法批量更新文章
        var result = await articleRepository.UpdateArticleOrders(
            command.Articles.ToDictionary(a => a.Path, a => 0)); // 使用临时顺序值
        
        if (result)
        {
            // 清除所有文章缓存
            await distributedCache.RemoveAsync(ArticlesCacheKey, cancellationToken);
            
            // 清除每个文章的单独缓存和分类缓存
            foreach (var article in command.Articles)
            {
                await ClearArticleCache(article.Path, article.CategoryId, cancellationToken);
            }
        }
        
        return result;
    }
    
    // 清除文章相关缓存的辅助方法
    private async Task ClearArticleCache(string articlePath, string? categoryId, CancellationToken cancellationToken)
    {
        // 清除所有文章缓存
        await distributedCache.RemoveAsync(ArticlesCacheKey, cancellationToken);
        
        // 清除单个文章缓存
        await distributedCache.RemoveAsync($"{ArticleCacheKeyPrefix}{articlePath}", cancellationToken);
        
        // 清除分类文章缓存
        if (!string.IsNullOrEmpty(categoryId))
        {
            await distributedCache.RemoveAsync($"{ArticlesByCategoryCacheKeyPrefix}{categoryId}", cancellationToken);
        }
        
        // 清除所有分类文章缓存，因为文章顺序可能会影响所有分类
        await ClearAllArticlesByCategoryCache(cancellationToken);
    }
    
    // 清除所有分类文章缓存的辅助方法
    private async Task ClearAllArticlesByCategoryCache(CancellationToken cancellationToken)
    {
        // 这里简化处理，实际项目中可能需要更精细的缓存键管理
        // 例如：维护一个所有分类文章缓存键的列表，或者使用缓存标签功能（如果缓存提供者支持）
        // 这里我们只清除已知的缓存键
        await distributedCache.RemoveAsync($"{ArticlesByCategoryCacheKeyPrefix}其他", cancellationToken);
    }
}