using Newtonsoft.Json;
using StackExchange.Redis;

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
public class DataAccessStatisticsService(IConnectionMultiplexer redis) : IDataAccessStatisticsService
{
    private const string AccessStatsKeyPrefix = "stats:access:";
    private const string ChangeStatsKeyPrefix = "stats:changes:";
    private const string LastAccessKeyPrefix = "stats:lastaccess:";
    private const string LastChangeKeyPrefix = "stats:lastchange:";
    private const string EntityTypesKey = "stats:entitytypes";
    private const int StatisticsExpirationDays = 30;

    private readonly IDatabase _db = redis.GetDatabase();

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

        // 获取Redis服务器实例
        var server = redis.GetServer(redis.GetEndPoints().First());

        foreach (var type in entityTypes)
        {
            var statistics = new List<DataAccessStatistic>();

            // 使用SCAN命令扫描指定前缀的键
            var pattern = $"{AccessStatsKeyPrefix}{type}:*";
            var keys = server.Keys(pattern: pattern, pageSize: 1000);

            foreach (var key in keys)
            {
                var keyStr = key.ToString();

                // 解析数据ID（格式: stats:access:entityType:dataId）
                var parts = keyStr.Split(':');
                if (parts.Length < 4) continue;

                var dataId = string.Join(":", parts.Skip(3));

                // 获取访问计数
                var countStr = await _db.StringGetAsync(keyStr);
                if (!long.TryParse((string?)countStr, out var count))
                {
                    count = 0;
                }

                // 获取最后访问时间
                var lastAccessKey = $"{LastAccessKeyPrefix}{type}:{dataId}";
                var lastAccessStr = await _db.StringGetAsync(lastAccessKey);
                DateTime? lastAccessedAt = null;
                if (!string.IsNullOrEmpty(lastAccessStr) && DateTime.TryParse(lastAccessStr, out var parsedTime))
                {
                    lastAccessedAt = parsedTime;
                }

                statistics.Add(new DataAccessStatistic
                {
                    DataId = dataId,
                    AccessCount = count,
                    LastAccessedAt = lastAccessedAt
                });
            }

            // 按访问次数降序排序并取前N条
            result.Statistics[type] = statistics
                .OrderByDescending(s => s.AccessCount)
                .Take(top)
                .ToList();
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

        // 获取Redis服务器实例
        var server = redis.GetServer(redis.GetEndPoints().First());

        foreach (var type in entityTypes)
        {
            var statistics = new List<DataChangeStatistic>();

            // 使用SCAN命令扫描指定前缀的键
            var pattern = $"{ChangeStatsKeyPrefix}{type}:*";
            var keys = server.Keys(pattern: pattern, pageSize: 1000);

            foreach (var key in keys)
            {
                var keyStr = key.ToString();

                // 解析数据ID（格式: stats:changes:entityType:dataId）
                var parts = keyStr.Split(':');
                if (parts.Length < 4) continue;

                var dataId = string.Join(":", parts.Skip(3));

                // 获取变化计数
                var countStr = await _db.StringGetAsync(keyStr);
                if (!long.TryParse((string?)countStr, out var count))
                {
                    count = 0;
                }

                // 获取最后变化时间
                var lastChangeKey = $"{LastChangeKeyPrefix}{type}:{dataId}";
                var lastChangeStr = await _db.StringGetAsync(lastChangeKey);
                DateTime? lastChangedAt = null;
                if (!string.IsNullOrEmpty(lastChangeStr) && DateTime.TryParse(lastChangeStr, out var parsedTime))
                {
                    lastChangedAt = parsedTime;
                }

                statistics.Add(new DataChangeStatistic
                {
                    DataId = dataId,
                    ChangeCount = count,
                    LastChangedAt = lastChangedAt
                });
            }

            // 按变化次数降序排序并取前N条
            result.Statistics[type] = statistics
                .OrderByDescending(s => s.ChangeCount)
                .Take(top)
                .ToList();
        }

        return result;
    }

    public async Task ResetStatisticsAsync(string? entityType = null, CancellationToken cancellationToken = default)
    {
        // 获取Redis服务器实例
        var server = redis.GetServer(redis.GetEndPoints().First());

        if (string.IsNullOrEmpty(entityType))
        {
            // 删除所有统计数据 - 使用SCAN扫描所有相关键
            var allPrefixes = new[]
            {
                AccessStatsKeyPrefix,
                ChangeStatsKeyPrefix,
                LastAccessKeyPrefix,
                LastChangeKeyPrefix
            };

            foreach (var prefix in allPrefixes)
            {
                var keys = server.Keys(pattern: $"{prefix}*", pageSize: 1000).ToArray();
                if (keys.Length > 0)
                {
                    await _db.KeyDeleteAsync(keys);
                }
            }

            // 删除实体类型列表
            await _db.KeyDeleteAsync(EntityTypesKey);
        }
        else
        {
            // 删除指定实体类型的统计数据
            entityType = entityType.ToLower();

            var patterns = new[]
            {
                $"{AccessStatsKeyPrefix}{entityType}:*",
                $"{ChangeStatsKeyPrefix}{entityType}:*",
                $"{LastAccessKeyPrefix}{entityType}:*",
                $"{LastChangeKeyPrefix}{entityType}:*"
            };

            foreach (var pattern in patterns)
            {
                var keys = server.Keys(pattern: pattern, pageSize: 1000).ToArray();
                if (keys.Length > 0)
                {
                    await _db.KeyDeleteAsync(keys);
                }
            }

            // 从实体类型列表中移除该类型
            var entityTypesJson = await _db.StringGetAsync(EntityTypesKey);
            if (!string.IsNullOrEmpty(entityTypesJson))
            {
                var entityTypes = JsonConvert.DeserializeObject<HashSet<string>>(entityTypesJson!) ?? new HashSet<string>();
                if (entityTypes.Remove(entityType))
                {
                    await _db.StringSetAsync(
                        EntityTypesKey,
                        JsonConvert.SerializeObject(entityTypes),
                        TimeSpan.FromDays(StatisticsExpirationDays));
                }
            }
        }
    }

    /// <summary>
    /// 递增计数器
    /// </summary>
    private async Task<long> IncrementCounterAsync(string key, CancellationToken cancellationToken = default)
    {
        // 使用Redis的原子递增操作
        var count = await _db.StringIncrementAsync(key);

        // 设置过期时间（如果key是新创建的）
        await _db.KeyExpireAsync(key, TimeSpan.FromDays(StatisticsExpirationDays));

        return count;
    }

    /// <summary>
    /// 更新时间戳
    /// </summary>
    private async Task UpdateTimestampAsync(string key, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        await _db.StringSetAsync(key, now.ToString("o"), TimeSpan.FromDays(StatisticsExpirationDays));
    }

    /// <summary>
    /// 确保实体类型被记录
    /// </summary>
    private async Task EnsureEntityTypeRecordedAsync(string entityType, CancellationToken cancellationToken = default)
    {
        var entityTypesJson = await _db.StringGetAsync(EntityTypesKey);
        var entityTypes = new HashSet<string>();

        if (!string.IsNullOrEmpty(entityTypesJson))
        {
            entityTypes = JsonConvert.DeserializeObject<HashSet<string>>(entityTypesJson!) ?? new HashSet<string>();
        }

        if (entityTypes.Add(entityType))
        {
            await _db.StringSetAsync(
                EntityTypesKey,
                JsonConvert.SerializeObject(entityTypes),
                TimeSpan.FromDays(StatisticsExpirationDays));
        }
    }

    /// <summary>
    /// 获取所有实体类型
    /// </summary>
    private async Task<List<string>> GetEntityTypesAsync(CancellationToken cancellationToken = default)
    {
        var entityTypesJson = await _db.StringGetAsync(EntityTypesKey);
        if (string.IsNullOrEmpty(entityTypesJson))
        {
            return [];
        }

        var entityTypes = JsonConvert.DeserializeObject<HashSet<string>>(entityTypesJson!) ?? new HashSet<string>();
        return entityTypes.ToList();
    }
}