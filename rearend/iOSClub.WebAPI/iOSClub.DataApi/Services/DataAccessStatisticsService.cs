using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace iOSClub.DataApi.Services;

/// <summary>
/// 数据访问统计服务，用于统计数据的访问频率和变化频率
/// </summary>
public interface IDataAccessStatisticsService
{
    /// <summary>
    /// 记录数据访问
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <param name="dataId">数据ID</param>
    /// <param name="operationType">操作类型（如"read", "write"）</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task RecordDataAccessAsync(string entityType, string dataId, string operationType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取数据访问统计
    /// </summary>
    /// <param name="entityType">实体类型（可选，为空则获取所有实体）</param>
    /// <param name="top">返回前N条数据</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>数据访问统计结果</returns>
    Task<DataAccessStatisticsResult> GetDataAccessStatisticsAsync(string? entityType = null, int top = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取数据变化统计
    /// </summary>
    /// <param name="entityType">实体类型（可选，为空则获取所有实体）</param>
    /// <param name="top">返回前N条数据</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>数据变化统计结果</returns>
    Task<DataChangeStatisticsResult> GetDataChangeStatisticsAsync(string? entityType = null, int top = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 重置统计数据
    /// </summary>
    /// <param name="entityType">实体类型（可选，为空则重置所有实体）</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task ResetStatisticsAsync(string? entityType = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// 数据访问统计结果
/// </summary>
public class DataAccessStatisticsResult
{
    /// <summary>
    /// 按实体类型分组的访问统计
    /// </summary>
    public Dictionary<string, List<DataAccessStatistic>> Statistics { get; set; } = [];

    /// <summary>
    /// 统计生成时间
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 数据访问统计项
/// </summary>
[Serializable]
public class DataAccessStatistic
{
    /// <summary>
    /// 数据ID
    /// </summary>
    public string DataId { get; set; } = string.Empty;

    /// <summary>
    /// 访问次数
    /// </summary>
    public long AccessCount { get; set; }

    /// <summary>
    /// 最后访问时间
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }
}

/// <summary>
/// 数据变化统计结果
/// </summary>
[Serializable]
public class DataChangeStatisticsResult
{
    /// <summary>
    /// 按实体类型分组的变化统计
    /// </summary>
    public Dictionary<string, List<DataChangeStatistic>> Statistics { get; set; } = [];

    /// <summary>
    /// 统计生成时间
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 数据变化统计项
/// </summary>
[Serializable]
public class DataChangeStatistic
{
    /// <summary>
    /// 数据ID
    /// </summary>
    public string DataId { get; set; } = string.Empty;

    /// <summary>
    /// 变化次数
    /// </summary>
    public long ChangeCount { get; set; }

    /// <summary>
    /// 最后变化时间
    /// </summary>
    public DateTime? LastChangedAt { get; set; }
}

/// <summary>
/// 数据访问统计服务实现
/// </summary>
public class DataAccessStatisticsService(IDistributedCache distributedCache) : IDataAccessStatisticsService
{
    private const string AccessStatsKeyPrefix = "stats:access:";
    private const string ChangeStatsKeyPrefix = "stats:changes:";
    private const string LastAccessKeyPrefix = "stats:lastaccess:";
    private const string LastChangeKeyPrefix = "stats:lastchange:";
    private const string EntityTypesKey = "stats:entitytypes";
    private const int StatisticsExpirationDays = 30;

    public async Task RecordDataAccessAsync(string entityType, string dataId, string operationType,
        CancellationToken cancellationToken = default)
    {
        // 标准化实体类型名称
        entityType = entityType.ToLower();

        // 确保实体类型被记录
        await EnsureEntityTypeRecordedAsync(entityType, cancellationToken);

        if (operationType == "read")
        {
            // 记录访问统计
            await IncrementCounterAsync($"{AccessStatsKeyPrefix}{entityType}:{dataId}", cancellationToken);
            await UpdateTimestampAsync($"{LastAccessKeyPrefix}{entityType}:{dataId}", cancellationToken);
        }
        else if (operationType is "write" or "create" or "update" or "delete")
        {
            // 记录变化统计
            await IncrementCounterAsync($"{ChangeStatsKeyPrefix}{entityType}:{dataId}", cancellationToken);
            await UpdateTimestampAsync($"{LastChangeKeyPrefix}{entityType}:{dataId}", cancellationToken);
        }
    }

    public async Task<DataAccessStatisticsResult> GetDataAccessStatisticsAsync(string? entityType = null, int top = 10,
        CancellationToken cancellationToken = default)
    {
        var result = new DataAccessStatisticsResult();

        // 获取实体类型列表
        var entityTypes = await GetEntityTypesAsync(cancellationToken);

        // 如果指定了实体类型，只处理该类型
        if (!string.IsNullOrEmpty(entityType))
        {
            entityTypes = entityTypes.Where(t => t == entityType.ToLower()).ToList();
        }

        foreach (var type in entityTypes)
        {
            // 获取该实体类型下的所有访问统计键
            // 注意：这里使用的是简化实现，实际生产环境中可能需要使用Redis的SCAN命令或维护索引
            // 为了简化，我们假设可以通过某种方式获取所有相关键
            // 这里我们使用一个模拟的方式，实际实现时需要根据具体的缓存实现来调整

            // 这里使用简化实现，直接返回空列表，后续会替换为实际实现
            result.Statistics[type] = [];
        }

        return result;
    }

    public async Task<DataChangeStatisticsResult> GetDataChangeStatisticsAsync(string? entityType = null, int top = 10,
        CancellationToken cancellationToken = default)
    {
        var result = new DataChangeStatisticsResult();

        // 获取实体类型列表
        var entityTypes = await GetEntityTypesAsync(cancellationToken);

        // 如果指定了实体类型，只处理该类型
        if (!string.IsNullOrEmpty(entityType))
        {
            entityTypes = entityTypes.Where(t => t == entityType.ToLower()).ToList();
        }

        foreach (var type in entityTypes)
        {
            // 获取该实体类型下的所有变化统计键
            // 同样使用简化实现
            result.Statistics[type] = [];
        }

        return result;
    }

    public async Task ResetStatisticsAsync(string? entityType = null, CancellationToken cancellationToken = default)
    {
        // 这里使用简化实现，实际生产环境中需要根据具体的缓存实现来调整
        // 例如，在Redis中可以使用DEL命令删除相关键
        if (string.IsNullOrEmpty(entityType))
        {
            // 删除所有统计数据
            await distributedCache.RemoveAsync(EntityTypesKey, cancellationToken);
            await distributedCache.RemoveAsync(AccessStatsKeyPrefix, cancellationToken);
            await distributedCache.RemoveAsync(ChangeStatsKeyPrefix, cancellationToken);
            await distributedCache.RemoveAsync(LastAccessKeyPrefix, cancellationToken);
            await distributedCache.RemoveAsync(LastChangeKeyPrefix, cancellationToken);
        }
        else
        {
            // 删除指定实体类型的统计数据
            await distributedCache.RemoveAsync($"{AccessStatsKeyPrefix}{entityType}", cancellationToken);
            await distributedCache.RemoveAsync($"{ChangeStatsKeyPrefix}{entityType}", cancellationToken);
            await distributedCache.RemoveAsync($"{LastAccessKeyPrefix}{entityType}", cancellationToken);
            await distributedCache.RemoveAsync($"{LastChangeKeyPrefix}{entityType}", cancellationToken);
        }
    }

    /// <summary>
    /// 递增计数器
    /// </summary>
    private async Task<long> IncrementCounterAsync(string key, CancellationToken cancellationToken = default)
    {
        // 简化实现：实际生产环境中应该使用原子递增操作
        // 这里使用分布式缓存的字符串操作来模拟
        var currentValue = await distributedCache.GetStringAsync(key, cancellationToken);
        long count = 0;

        if (long.TryParse(currentValue, out var value))
        {
            count = value;
        }

        count++;

        await distributedCache.SetStringAsync(
            key,
            count.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(StatisticsExpirationDays)
            },
            cancellationToken);

        return count;
    }

    /// <summary>
    /// 更新时间戳
    /// </summary>
    private async Task UpdateTimestampAsync(string key, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        await distributedCache.SetStringAsync(
            key,
            now.ToString("o"),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(StatisticsExpirationDays)
            },
            cancellationToken);
    }

    /// <summary>
    /// 确保实体类型被记录
    /// </summary>
    private async Task EnsureEntityTypeRecordedAsync(string entityType, CancellationToken cancellationToken = default)
    {
        var entityTypesJson = await distributedCache.GetStringAsync(EntityTypesKey, cancellationToken);
        var entityTypes = new HashSet<string>();

        if (!string.IsNullOrEmpty(entityTypesJson))
        {
            entityTypes = JsonConvert.DeserializeObject<HashSet<string>>(entityTypesJson) ?? new HashSet<string>();
        }

        if (entityTypes.Add(entityType))
        {
            await distributedCache.SetStringAsync(
                EntityTypesKey,
                JsonConvert.SerializeObject(entityTypes),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(StatisticsExpirationDays)
                },
                cancellationToken);
        }
    }

    /// <summary>
    /// 获取所有实体类型
    /// </summary>
    private async Task<List<string>> GetEntityTypesAsync(CancellationToken cancellationToken = default)
    {
        var entityTypesJson = await distributedCache.GetStringAsync(EntityTypesKey, cancellationToken);
        if (string.IsNullOrEmpty(entityTypesJson))
        {
            return [];
        }

        var entityTypes = JsonConvert.DeserializeObject<HashSet<string>>(entityTypesJson) ?? new HashSet<string>();
        return entityTypes.ToList();
    }
}