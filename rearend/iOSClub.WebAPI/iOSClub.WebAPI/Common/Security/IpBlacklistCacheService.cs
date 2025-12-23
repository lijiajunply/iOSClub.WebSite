using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace iOSClub.WebAPI.Common.Security;

/// <summary>
/// IP黑名单缓存服务接口
/// </summary>
public interface IIpBlacklistCacheService
{
    /// <summary>
    /// 检查IP是否在黑名单中
    /// </summary>
    Task<bool> IsIpBlacklistedAsync(string ip);

    /// <summary>
    /// 刷新黑名单缓存
    /// </summary>
    Task RefreshBlacklistAsync();
}

/// <summary>
/// IP黑名单缓存服务实现
/// </summary>
public class IpBlacklistCacheService : IIpBlacklistCacheService
{
    private const string CacheKey = "ip_blacklist";
    private const int CacheExpirationMinutes = 20;

    private readonly IDistributedCache _cache;
    private readonly ILogger<IpBlacklistCacheService> _logger;
    private readonly IConfiguration _configuration;

    public IpBlacklistCacheService(
        IDistributedCache cache,
        ILogger<IpBlacklistCacheService> logger,
        IConfiguration configuration)
    {
        _cache = cache;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// 从数据源加载IP黑名单
    /// </summary>
    private async Task<HashSet<string>> LoadBlacklistFromSourceAsync()
    {
        var blacklist = new HashSet<string>();

        try
        {
            // 从配置文件加载黑名单（支持环境变量和appsettings.json）
            var configBlacklist = _configuration.GetSection("Security:IpBlacklist").Get<string[]>();
            if (configBlacklist != null && configBlacklist.Length > 0)
            {
                foreach (var ip in configBlacklist)
                {
                    if (!string.IsNullOrWhiteSpace(ip))
                    {
                        blacklist.Add(ip.Trim());
                    }
                }
            }

            // 从环境变量加载（逗号分隔）
            var envBlacklist = Environment.GetEnvironmentVariable("IP_BLACKLIST");
            if (!string.IsNullOrWhiteSpace(envBlacklist))
            {
                var ips = envBlacklist.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var ip in ips)
                {
                    blacklist.Add(ip.Trim());
                }
            }

            _logger.LogInformation("从数据源加载了 {Count} 个IP到黑名单", blacklist.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载IP黑名单时发生错误");
        }

        return await Task.FromResult(blacklist);
    }

    /// <summary>
    /// 从缓存获取IP黑名单
    /// </summary>
    private async Task<HashSet<string>?> GetBlacklistFromCacheAsync()
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(CacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                var blacklist = JsonSerializer.Deserialize<HashSet<string>>(cachedData);
                _logger.LogDebug("从Redis缓存读取到 {Count} 个IP黑名单记录", blacklist?.Count ?? 0);
                return blacklist;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从Redis缓存读取IP黑名单时发生错误");
        }

        return null;
    }

    /// <summary>
    /// 将IP黑名单保存到缓存
    /// </summary>
    private async Task SaveBlacklistToCacheAsync(HashSet<string> blacklist)
    {
        try
        {
            var jsonData = JsonSerializer.Serialize(blacklist);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes)
            };

            await _cache.SetStringAsync(CacheKey, jsonData, options);
            _logger.LogInformation("成功将 {Count} 个IP黑名单记录保存到Redis缓存，过期时间：{Minutes}分钟",
                blacklist.Count, CacheExpirationMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存IP黑名单到Redis缓存时发生错误");
        }
    }

    /// <summary>
    /// 检查IP是否在黑名单中
    /// </summary>
    public async Task<bool> IsIpBlacklistedAsync(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            return false;
        }

        try
        {
            // 先从缓存获取
            var blacklist = await GetBlacklistFromCacheAsync();

            // 如果缓存中没有，从数据源加载并缓存
            if (blacklist == null)
            {
                blacklist = await LoadBlacklistFromSourceAsync();
                await SaveBlacklistToCacheAsync(blacklist);
            }

            return blacklist.Contains(ip);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查IP黑名单时发生错误：{Ip}", ip);
            return false;
        }
    }

    /// <summary>
    /// 刷新黑名单缓存
    /// </summary>
    public async Task RefreshBlacklistAsync()
    {
        try
        {
            _logger.LogInformation("开始刷新IP黑名单缓存");

            // 从数据源重新加载
            var blacklist = await LoadBlacklistFromSourceAsync();

            // 保存到缓存
            await SaveBlacklistToCacheAsync(blacklist);

            _logger.LogInformation("IP黑名单缓存刷新完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新IP黑名单缓存时发生错误");
        }
    }
}
