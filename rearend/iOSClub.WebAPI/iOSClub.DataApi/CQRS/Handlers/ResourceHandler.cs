using iOSClub.Data.DataModels;
using iOSClub.DataApi.CQRS.Commands;
using iOSClub.DataApi.CQRS.Queries;
using iOSClub.DataApi.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace iOSClub.DataApi.CQRS.Handlers;

public class ResourceQueryHandler(IResourceRepository resourceRepository, IDistributedCache distributedCache) : 
    IQueryHandler<GetResourcesQuery, IEnumerable<ResourceModel>>,
    IQueryHandler<GetResourceByIdQuery, ResourceModel?>,
    IQueryHandler<GetResourcesByTagQuery, IEnumerable<ResourceModel>>,
    IQueryHandler<SearchResourcesByNameQuery, IEnumerable<ResourceModel>>
{
    private const string ResourcesCacheKey = "resources:all";
    private const string ResourceCacheKeyPrefix = "resources:";
    private const string ResourcesByTagCacheKeyPrefix = "resources:tag:";
    private const string SearchResourcesCacheKeyPrefix = "resources:search:";
    private const int CacheExpirationMinutes = 30;

    public async Task<IEnumerable<ResourceModel>> Handle(GetResourcesQuery query, CancellationToken cancellationToken = default)
    {
        // 尝试从缓存获取
        var cachedResources = await distributedCache.GetStringAsync(ResourcesCacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedResources))
        {
            return JsonConvert.DeserializeObject<IEnumerable<ResourceModel>>(cachedResources)!;
        }

        // 缓存不存在，从数据库获取
        var resources = await resourceRepository.GetAllResourcesAsync();

        // 存入缓存
        await distributedCache.SetStringAsync(
            ResourcesCacheKey,
            JsonConvert.SerializeObject(resources),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
            cancellationToken);

        return resources;
    }

    public async Task<ResourceModel?> Handle(GetResourceByIdQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ResourceCacheKeyPrefix}{query.Id}";
        
        // 尝试从缓存获取
        var cachedResource = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedResource))
        {
            return JsonConvert.DeserializeObject<ResourceModel>(cachedResource);
        }

        // 缓存不存在，从数据库获取
        var resource = await resourceRepository.GetResourceByIdAsync(query.Id);

        if (resource != null)
        {
            // 存入缓存
            await distributedCache.SetStringAsync(
                cacheKey,
                JsonConvert.SerializeObject(resource),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                cancellationToken);
        }

        return resource;
    }

    public async Task<IEnumerable<ResourceModel>> Handle(GetResourcesByTagQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ResourcesByTagCacheKeyPrefix}{query.Tag}";
        
        // 尝试从缓存获取
        var cachedResources = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedResources))
        {
            return JsonConvert.DeserializeObject<IEnumerable<ResourceModel>>(cachedResources)!;
        }

        // 缓存不存在，从数据库获取
        var resources = await resourceRepository.GetResourcesByTagAsync(query.Tag);

        // 存入缓存
        await distributedCache.SetStringAsync(
            cacheKey,
            JsonConvert.SerializeObject(resources),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
            cancellationToken);

        return resources;
    }

    public async Task<IEnumerable<ResourceModel>> Handle(SearchResourcesByNameQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{SearchResourcesCacheKeyPrefix}{query.Name}";
        
        // 尝试从缓存获取
        var cachedResources = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedResources))
        {
            return JsonConvert.DeserializeObject<IEnumerable<ResourceModel>>(cachedResources)!;
        }

        // 缓存不存在，从数据库获取
        var resources = await resourceRepository.SearchResourcesByNameAsync(query.Name);

        // 存入缓存，搜索结果缓存时间较短
        await distributedCache.SetStringAsync(
            cacheKey,
            JsonConvert.SerializeObject(resources),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) },
            cancellationToken);

        return resources;
    }
}

public class ResourceCommandHandler(IResourceRepository resourceRepository, IDistributedCache distributedCache) : 
    ICommandHandler<CreateResourceCommand, bool>,
    ICommandHandler<UpdateResourceCommand, bool>,
    ICommandHandler<DeleteResourceCommand, bool>
{
    private const string ResourcesCacheKey = "resources:all";
    private const string ResourceCacheKeyPrefix = "resources:";
    private const string ResourcesByTagCacheKeyPrefix = "resources:tag:";
    private const string SearchResourcesCacheKeyPrefix = "resources:search:";

    public async Task<bool> Handle(CreateResourceCommand command, CancellationToken cancellationToken = default)
    {
        var result = await resourceRepository.AddResourceAsync(command.Resource);
        
        if (result)
        {
            // 清除相关缓存
            await ClearResourceCache(command.Resource.Id, cancellationToken);
        }
        
        return result;
    }

    public async Task<bool> Handle(UpdateResourceCommand command, CancellationToken cancellationToken = default)
    {
        var result = await resourceRepository.UpdateResourceAsync(command.Resource);
        
        if (result)
        {
            // 清除相关缓存
            await ClearResourceCache(command.Resource.Id, cancellationToken);
        }
        
        return result;
    }

    public async Task<bool> Handle(DeleteResourceCommand command, CancellationToken cancellationToken = default)
    {
        var result = await resourceRepository.DeleteResourceAsync(command.Id);
        
        if (result)
        {
            // 清除相关缓存
            await ClearResourceCache(command.Id, cancellationToken);
        }
        
        return result;
    }

    // 清除资源相关缓存的辅助方法
    private async Task ClearResourceCache(string resourceId, CancellationToken cancellationToken)
    {
        // 清除所有资源缓存
        await distributedCache.RemoveAsync(ResourcesCacheKey, cancellationToken);
        
        // 清除单个资源缓存
        await distributedCache.RemoveAsync($"{ResourceCacheKeyPrefix}{resourceId}", cancellationToken);
        
        // 注意：在实际生产环境中，可能需要更精细地清除标签和搜索相关缓存
        // 这里为了简单起见，只清除了主要缓存
    }
}