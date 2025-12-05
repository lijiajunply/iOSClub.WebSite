using System.Threading.RateLimiting;

namespace iOSClub.WebAPI.Common.Security;

/// <summary>
/// API速率限制策略
/// </summary>
public class RateLimitPolicy
{
    /// <summary>
    /// 策略名称
    /// </summary>
    public string Name { get; set; } = "default";

    /// <summary>
    /// 匹配的API路径模式（支持通配符*）
    /// </summary>
    public string PathPattern { get; set; } = "*";

    /// <summary>
    /// 令牌桶容量
    /// </summary>
    public int TokenLimit { get; set; } = 100;

    /// <summary>
    /// 令牌补充周期
    /// </summary>
    public TimeSpan ReplenishmentPeriod { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// 每个周期补充的令牌数
    /// </summary>
    public int TokensPerPeriod { get; set; } = 100;

    /// <summary>
    /// 是否自动补充令牌
    /// </summary>
    public bool AutoReplenishment { get; set; } = true;

    /// <summary>
    /// 是否启用该策略
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 限流策略优先级（数值越小优先级越高）
    /// </summary>
    public int Priority { get; set; } = 100;
}

/// <summary>
/// 速率限制配置
/// </summary>
public class RateLimitConfig
{
    /// <summary>
    /// 是否启用速率限制
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 是否启用动态限流调整
    /// </summary>
    public bool EnableDynamicAdjustment { get; set; } = true;

    /// <summary>
    /// 动态调整间隔
    /// </summary>
    public TimeSpan DynamicAdjustmentInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 系统负载阈值（超过该阈值将触发动态限流调整）
    /// </summary>
    public double SystemLoadThreshold { get; set; } = 0.8;

    /// <summary>
    /// 限流策略列表
    /// </summary>
    public List<RateLimitPolicy> Policies { get; set; } = new List<RateLimitPolicy>
    {
        // 默认策略
        new RateLimitPolicy
        {
            Name = "default",
            PathPattern = "*",
            TokenLimit = 100,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 100,
            AutoReplenishment = true,
            Enabled = true,
            Priority = 100
        },
        // 登录接口策略（更严格）
        new RateLimitPolicy
        {
            Name = "login",
            PathPattern = "/api/auth/*",
            TokenLimit = 10,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 10,
            AutoReplenishment = true,
            Enabled = true,
            Priority = 10
        },
        // 注册接口策略（更严格）
        new RateLimitPolicy
        {
            Name = "register",
            PathPattern = "/api/users/register",
            TokenLimit = 5,
            ReplenishmentPeriod = TimeSpan.FromMinutes(5),
            TokensPerPeriod = 5,
            AutoReplenishment = true,
            Enabled = true,
            Priority = 10
        },
        // 敏感操作接口策略
        new RateLimitPolicy
        {
            Name = "sensitive",
            PathPattern = "/api/admin/*",
            TokenLimit = 50,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 50,
            AutoReplenishment = true,
            Enabled = true,
            Priority = 50
        }
    };
}

/// <summary>
/// 速率限制状态
/// </summary>
public class RateLimitState
{
    /// <summary>
    /// 当前系统负载
    /// </summary>
    public double CurrentSystemLoad { get; set; } = 0;

    /// <summary>
    /// 最后调整时间
    /// </summary>
    public DateTime LastAdjustmentTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 当前激活的策略
    /// </summary>
    public Dictionary<string, RateLimitPolicy> ActivePolicies { get; set; } = new Dictionary<string, RateLimitPolicy>();
}

/// <summary>
/// 速率限制服务
/// </summary>
public class RateLimitService
{
    private readonly RateLimitConfig _config;
    private readonly RateLimitState _state;
    private readonly ILogger<RateLimitService> _logger;

    private readonly Dictionary<string, TokenBucketRateLimiter> _limiters =
        new Dictionary<string, TokenBucketRateLimiter>();

    public RateLimitService(RateLimitConfig config, ILogger<RateLimitService> logger)
    {
        _config = config;
        _logger = logger;
        _state = new RateLimitState();

        // 初始化限流器
        InitializeLimiters();
    }

    /// <summary>
    /// 初始化限流器
    /// </summary>
    private void InitializeLimiters()
    {
        foreach (var policy in _config.Policies.OrderBy(p => p.Priority))
        {
            if (policy.Enabled && !_limiters.ContainsKey(policy.Name))
            {
                var limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
                {
                    TokenLimit = policy.TokenLimit,
                    ReplenishmentPeriod = policy.ReplenishmentPeriod,
                    TokensPerPeriod = policy.TokensPerPeriod,
                    AutoReplenishment = policy.AutoReplenishment
                });

                _limiters.Add(policy.Name, limiter);
                _state.ActivePolicies[policy.Name] = policy;
            }
        }
    }

    /// <summary>
    /// 获取匹配的速率限制策略
    /// </summary>
    /// <param name="path">API路径</param>
    /// <returns>匹配的策略</returns>
    public RateLimitPolicy GetMatchingPolicy(string path)
    {
        // 按照优先级排序，优先匹配优先级高的策略
        foreach (var policy in _config.Policies.OrderBy(p => p.Priority))
        {
            if (policy.Enabled && IsPathMatch(policy.PathPattern, path))
            {
                return policy;
            }
        }

        // 默认返回默认策略
        return _config.Policies.FirstOrDefault(p => p.Name == "default") ?? _config.Policies[0];
    }

    /// <summary>
    /// 检查路径是否匹配模式
    /// </summary>
    /// <param name="pattern">路径模式</param>
    /// <param name="path">API路径</param>
    /// <returns>是否匹配</returns>
    private bool IsPathMatch(string pattern, string path)
    {
        // 简单的通配符匹配实现
        var regexPattern = pattern.Replace("*", ".*");
        return System.Text.RegularExpressions.Regex.IsMatch(path, regexPattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// 尝试获取令牌
    /// </summary>
    /// <param name="policy">速率限制策略</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>令牌租约</returns>
    public async ValueTask<RateLimitLease> TryAcquireTokenAsync(RateLimitPolicy policy,
        CancellationToken cancellationToken = default)
    {
        if (!_limiters.TryGetValue(policy.Name, out var limiter))
        {
            // 如果限流器不存在，创建一个
            limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = policy.TokenLimit,
                ReplenishmentPeriod = policy.ReplenishmentPeriod,
                TokensPerPeriod = policy.TokensPerPeriod,
                AutoReplenishment = policy.AutoReplenishment
            });

            _limiters[policy.Name] = limiter;
        }

        return await limiter.AcquireAsync(1, cancellationToken);
    }

    /// <summary>
    /// 动态调整限流阈值
    /// </summary>
    public void AdjustRateLimitsDynamically()
    {
        if (!_config.EnableDynamicAdjustment)
            return;

        // 检查是否需要调整（根据时间间隔）
        var timeSinceLastAdjustment = DateTime.UtcNow - _state.LastAdjustmentTime;
        if (timeSinceLastAdjustment < _config.DynamicAdjustmentInterval)
            return;

        try
        {
            // 获取当前系统负载（这里使用模拟值，实际项目中可以从系统监控获取）
            var currentLoad = GetCurrentSystemLoad();
            _state.CurrentSystemLoad = currentLoad;

            _logger.LogInformation("动态调整限流阈值，当前系统负载: {CurrentLoad:P2}", currentLoad);

            // 根据系统负载调整限流阈值
            foreach (var policy in _config.Policies)
            {
                if (!policy.Enabled)
                    continue;
                    
                // 检查策略名称是否有效
                if (string.IsNullOrEmpty(policy.Name))
                {
                    _logger.LogWarning("跳过无效的限流策略：策略名称为空");
                    continue;
                }

                // 计算新的令牌限制（基于当前负载）
                var newTokenLimit = (int)(policy.TokenLimit * (1 - currentLoad * 0.5));
                newTokenLimit = Math.Max(newTokenLimit, policy.TokenLimit / 2); // 确保不低于原限制的一半
                newTokenLimit = Math.Max(newTokenLimit, 10); // 确保不低于10

                // 更新策略
                policy.TokenLimit = newTokenLimit;
                policy.TokensPerPeriod = newTokenLimit;

                // 重新创建限流器
                if (_limiters.TryGetValue(policy.Name, out var existingLimiter))
                {
                    existingLimiter.Dispose();
                    _limiters.Remove(policy.Name);
                }

                var newLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
                {
                    TokenLimit = policy.TokenLimit,
                    ReplenishmentPeriod = policy.ReplenishmentPeriod,
                    TokensPerPeriod = policy.TokensPerPeriod,
                    AutoReplenishment = policy.AutoReplenishment
                });

                _limiters[policy.Name] = newLimiter;
                _state.ActivePolicies[policy.Name] = policy;

                _logger.LogInformation("更新策略 {PolicyName} 的限流阈值为 {NewTokenLimit}", policy.Name, newTokenLimit);
            }

            // 更新最后调整时间
            _state.LastAdjustmentTime = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "动态调整限流阈值失败");
        }
    }

    /// <summary>
    /// 获取当前系统负载（模拟实现）
    /// </summary>
    /// <returns>系统负载（0-1之间）</returns>
    private double GetCurrentSystemLoad()
    {
        // 实际项目中可以使用以下方式获取系统负载：
        // 1. 对于Windows，可以使用PerformanceCounter
        // 2. 对于Linux，可以读取/proc/loadavg
        // 3. 可以从监控系统获取
        // 这里使用随机值模拟
        var random = new Random();
        return random.NextDouble() * 0.5; // 生成0-0.5之间的随机负载
    }

    /// <summary>
    /// 记录请求统计信息
    /// </summary>
    /// <param name="clientIp">客户端IP</param>
    /// <param name="path">请求路径</param>
    /// <param name="isAllowed">是否允许请求</param>
    public void RecordRequest(string clientIp, string path, bool isAllowed)
    {
        // 这里可以实现请求统计逻辑，例如：
        // 1. 记录到数据库
        // 2. 记录到Redis用于实时监控
        // 3. 用于异常请求检测

        _logger.LogDebug("请求统计: IP={ClientIp}, Path={Path}, Allowed={IsAllowed}", clientIp, path, isAllowed);
    }
}