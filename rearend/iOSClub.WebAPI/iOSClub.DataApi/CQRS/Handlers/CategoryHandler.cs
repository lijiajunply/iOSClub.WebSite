using iOSClub.Data.DataModels;
using iOSClub.DataApi.CQRS.Commands;
using iOSClub.DataApi.CQRS.Queries;
using iOSClub.DataApi.Repositories;
using iOSClub.DataApi.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace iOSClub.DataApi.CQRS.Handlers;

public class CategoryQueryHandler(
    ICategoryRepository categoryRepository,
    IDistributedCache distributedCache,
    IDataAccessStatisticsService statisticsService) :
    IQueryHandler<GetCategoriesQuery, IEnumerable<CategoryModel>>,
    IQueryHandler<GetCategoryByIdQuery, CategoryModel?>,
    IQueryHandler<GetCategoryByNameQuery, CategoryModel?>
{
    private const string CategoriesCacheKey = "categories:all";
    private const string CategoryCacheKeyPrefix = "categories:";
    private const string CategoryByNameCacheKeyPrefix = "categories:name:";
    private const int CacheExpirationMinutes = 30;

    public async Task<IEnumerable<CategoryModel>> Handle(GetCategoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        // 尝试从缓存获取
        var cachedCategories = await distributedCache.GetStringAsync(CategoriesCacheKey, cancellationToken);
        IEnumerable<CategoryModel> categories;

        if (!string.IsNullOrEmpty(cachedCategories))
        {
            categories = JsonConvert.DeserializeObject<IEnumerable<CategoryModel>>(cachedCategories)!;
        }
        else
        {
            // 缓存不存在，从数据库获取
            categories = await categoryRepository.GetAll();

            // 存入缓存
            await distributedCache.SetStringAsync(
                CategoriesCacheKey,
                JsonConvert.SerializeObject(categories),
                new DistributedCacheEntryOptions
                    { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                cancellationToken);
        }

        // 记录访问统计
        await statisticsService.RecordDataAccessAsync("category", "all", "read", cancellationToken);

        return categories;
    }

    public async Task<CategoryModel?> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CategoryCacheKeyPrefix}{query.Id}";

        // 尝试从缓存获取
        var cachedCategory = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        CategoryModel? category;

        if (!string.IsNullOrEmpty(cachedCategory))
        {
            category = JsonConvert.DeserializeObject<CategoryModel>(cachedCategory);
        }
        else
        {
            // 缓存不存在，从数据库获取
            category = await categoryRepository.GetById(query.Id);

            if (category != null)
            {
                // 存入缓存
                await distributedCache.SetStringAsync(
                    cacheKey,
                    JsonConvert.SerializeObject(category),
                    new DistributedCacheEntryOptions
                        { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                    cancellationToken);
            }
        }

        // 记录访问统计
        await statisticsService.RecordDataAccessAsync("category", query.Id, "read", cancellationToken);

        return category;
    }

    public async Task<CategoryModel?> Handle(GetCategoryByNameQuery query,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CategoryByNameCacheKeyPrefix}{query.Name}";

        // 尝试从缓存获取
        var cachedCategory = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        CategoryModel? category;

        if (!string.IsNullOrEmpty(cachedCategory))
        {
            category = JsonConvert.DeserializeObject<CategoryModel>(cachedCategory);
        }
        else
        {
            // 缓存不存在，从数据库获取
            category = await categoryRepository.GetByName(query.Name);

            if (category != null)
            {
                // 存入缓存
                await distributedCache.SetStringAsync(
                    cacheKey,
                    JsonConvert.SerializeObject(category),
                    new DistributedCacheEntryOptions
                        { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                    cancellationToken);
            }
        }

        // 记录访问统计
        await statisticsService.RecordDataAccessAsync("category", $"name:{query.Name}", "read", cancellationToken);

        return category;
    }
}

public class CategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IDistributedCache distributedCache,
    IDataAccessStatisticsService statisticsService) :
    ICommandHandler<CreateCategoryCommand, bool>,
    ICommandHandler<UpdateCategoryCommand, bool>,
    ICommandHandler<DeleteCategoryCommand, bool>,
    ICommandHandler<UpdateManyCategoriesCommand, bool>
{
    private const string CategoriesCacheKey = "categories:all";
    private const string CategoryCacheKeyPrefix = "categories:";
    private const string CategoryByNameCacheKeyPrefix = "categories:name:";
    private const string ArticlesByCategoryCacheKeyPrefix = "articles:category:";

    public async Task<bool> Handle(CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        // 创建或更新分类
        var result = await categoryRepository.CreateOrUpdate(command.Category);

        if (result)
        {
            // 清除相关缓存
            await ClearCategoryCache(command.Category, cancellationToken);

            // 记录变化统计
            await statisticsService.RecordDataAccessAsync("category", command.Category.Id, "create", cancellationToken);
        }

        return result;
    }

    public async Task<bool> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        // 更新分类
        var result = await categoryRepository.CreateOrUpdate(command.Category);

        if (result)
        {
            // 清除相关缓存
            await ClearCategoryCache(command.Category, cancellationToken);

            // 记录变化统计
            await statisticsService.RecordDataAccessAsync("category", command.Category.Id, "update", cancellationToken);
        }

        return result;
    }

    public async Task<bool> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
    {
        // 删除分类
        var result = await categoryRepository.Delete(command.Id);

        if (result)
        {
            // 清除相关缓存
            await ClearCategoryCacheById(command.Id, cancellationToken);

            // 记录变化统计
            await statisticsService.RecordDataAccessAsync("category", command.Id, "delete", cancellationToken);
        }

        return result;
    }

    public async Task<bool> Handle(UpdateManyCategoriesCommand command, CancellationToken cancellationToken = default)
    {
        // 批量更新分类
        var categoryOrders = command.Categories.ToDictionary(c => c.Id, c => c.Order);
        var result = await categoryRepository.UpdateCategoryOrders(categoryOrders);

        if (result)
        {
            // 清除所有分类缓存
            await distributedCache.RemoveAsync(CategoriesCacheKey, cancellationToken);

            // 清除每个分类的单独缓存并记录变化统计
            foreach (var category in command.Categories)
            {
                await ClearCategoryCache(category, cancellationToken);
                await statisticsService.RecordDataAccessAsync("category", category.Id, "update", cancellationToken);
            }
        }

        return result;
    }

    // 清除分类相关缓存的辅助方法
    private async Task ClearCategoryCache(CategoryModel category, CancellationToken cancellationToken)
    {
        // 清除所有分类缓存
        await distributedCache.RemoveAsync(CategoriesCacheKey, cancellationToken);

        // 清除单个分类缓存
        await distributedCache.RemoveAsync($"{CategoryCacheKeyPrefix}{category.Id}", cancellationToken);

        // 清除按名称缓存的分类
        await distributedCache.RemoveAsync($"{CategoryByNameCacheKeyPrefix}{category.Name}", cancellationToken);

        // 清除相关的文章分类缓存
        await distributedCache.RemoveAsync($"{ArticlesByCategoryCacheKeyPrefix}{category.Id}", cancellationToken);
    }

    // 根据ID清除分类相关缓存的辅助方法
    private async Task ClearCategoryCacheById(string categoryId, CancellationToken cancellationToken)
    {
        // 清除所有分类缓存
        await distributedCache.RemoveAsync(CategoriesCacheKey, cancellationToken);

        // 清除单个分类缓存
        await distributedCache.RemoveAsync($"{CategoryCacheKeyPrefix}{categoryId}", cancellationToken);

        // 清除相关的文章分类缓存
        await distributedCache.RemoveAsync($"{ArticlesByCategoryCacheKeyPrefix}{categoryId}", cancellationToken);
    }
}