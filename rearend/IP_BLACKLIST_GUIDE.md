# IP 黑名单系统 - 优化版使用文档

## 📋 功能特性

### ✅ 已实现的优化

| 功能 | 说明 | 优先级 |
|------|------|--------|
| **双层缓存** | 内存缓存(2分钟) + Redis缓存(20分钟) | P1 |
| **并发安全** | SemaphoreSlim 防止缓存击穿 | P0 |
| **动态管理** | API 接口动态添加/删除黑名单 | P2 |
| **IP段匹配** | 支持 CIDR 格式(如 192.168.1.0/24) | P3 |
| **监控指标** | 缓存命中率、黑名单命中数统计 | P4 |
| **优雅降级** | Redis 故障时自动降级到内存 | P4 |

---

## 🚀 快速开始

### 1. 配置黑名单

#### 方式一：通过 appsettings.json
```json
{
  "Security": {
    "IpBlacklist": [
      "192.168.1.100",          // 单个IP
      "10.0.0.50",
      "172.16.0.0/24"           // CIDR范围
    ]
  }
}
```

#### 方式二：通过环境变量
```bash
export IP_BLACKLIST="192.168.1.100,10.0.0.50,172.16.0.0/24"
```

### 2. 使用管理 API

#### 🔐 认证要求
所有管理接口需要 `Admin` 或 `Founder` 角色。

#### 📊 获取统计信息
```bash
GET /api/IpBlacklist/stats
Authorization: Bearer {your-jwt-token}

# 响应示例
{
  "code": 200,
  "data": {
    "totalIps": 5,
    "totalCidrRanges": 2,
    "cacheHits": 10234,
    "cacheMisses": 156,
    "blacklistHits": 42,
    "lastRefreshTime": "2025-12-23T10:30:00Z"
  }
}
```

#### ➕ 添加 IP 到黑名单
```bash
POST /api/IpBlacklist/add
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "ip": "192.168.1.100",
  "reason": "恶意攻击"
}

# 或添加 CIDR 范围
{
  "ip": "192.168.1.0/24",
  "reason": "可疑网段"
}
```

#### ➖ 从黑名单移除 IP
```bash
POST /api/IpBlacklist/remove
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "ip": "192.168.1.100",
  "reason": "误封，已核实"
}
```

#### 🔄 手动刷新缓存
```bash
POST /api/IpBlacklist/refresh
Authorization: Bearer {your-jwt-token}

# 场景：配置文件更新后需要立即生效
```

#### 🔍 检查 IP 是否在黑名单
```bash
GET /api/IpBlacklist/check/192.168.1.100
Authorization: Bearer {your-jwt-token}

# 响应
{
  "code": 200,
  "data": {
    "ip": "192.168.1.100",
    "isBlacklisted": true,
    "checkTime": "2025-12-23T10:35:00Z"
  }
}
```

---

## 🎯 架构设计

### 缓存层级

```
请求 → 内存缓存(2分钟) → Redis缓存(20分钟) → 配置文件/环境变量
         ↓ 命中                ↓ 命中              ↓ 加载
       返回结果            返回结果          返回结果并缓存
```

### 并发控制流程

```
多个并发请求
    ↓
检查内存缓存 → 命中 → 返回
    ↓ 未命中
获取信号量锁 (只允许一个线程通过)
    ↓
双重检查内存缓存
    ↓
加载数据 → 保存到内存和Redis
    ↓
释放锁
    ↓
其他等待线程直接从内存缓存获取
```

### IP 匹配逻辑

```
检查 IP
    ↓
1. 精确匹配 (HashSet.Contains)
    ↓ 未匹配
2. CIDR 范围匹配 (逐个检查)
    ↓
返回结果
```

---

## 📊 性能特性

### 缓存效率
- **内存缓存命中**：< 1ms
- **Redis 缓存命中**：< 5ms
- **配置加载**：< 50ms

### 并发保护
- 使用 `SemaphoreSlim` 防止缓存击穿
- 支持万级并发请求
- 避免"惊群效应"

### 优雅降级
```
Redis 正常 → 使用双层缓存
    ↓
Redis 故障 → 自动降级到内存缓存
    ↓
继续提供服务（日志记录警告）
```

---

## 🔧 配置说明

### 缓存过期时间

在 [IpBlacklistCacheService.cs](rearend/iOSClub.WebAPI/iOSClub.WebAPI/Common/Security/IpBlacklistCacheService.cs) 中修改：

```csharp
private const int RedisCacheExpirationMinutes = 20;     // Redis 缓存过期时间
private const int MemoryCacheExpirationMinutes = 2;     // 内存缓存过期时间
```

### CIDR 格式说明

| CIDR | IP 范围 | 说明 |
|------|---------|------|
| 192.168.1.0/32 | 192.168.1.0 | 单个IP |
| 192.168.1.0/24 | 192.168.1.0 - 192.168.1.255 | 256个IP |
| 192.168.0.0/16 | 192.168.0.0 - 192.168.255.255 | 65536个IP |
| 10.0.0.0/8 | 10.0.0.0 - 10.255.255.255 | 16777216个IP |

---

## 📈 监控指标

### 统计指标说明

| 指标 | 说明 |
|------|------|
| `TotalIps` | 精确IP数量 |
| `TotalCidrRanges` | CIDR范围数量 |
| `CacheHits` | 缓存命中次数(内存) |
| `CacheMisses` | 缓存未命中次数 |
| `BlacklistHits` | 黑名单拦截次数 |
| `LastRefreshTime` | 最后刷新时间 |

### 健康检查建议

- **缓存命中率** = CacheHits / (CacheHits + CacheMisses)
  - 目标：> 95%
  - 异常：< 80% (考虑增加内存缓存时间)

- **黑名单命中率** = BlacklistHits / 总请求数
  - 正常：< 1%
  - 警告：> 5% (可能遭受攻击)

---

## 🛠️ 故障排查

### 问题：Redis 连接失败

**现象**：日志中出现 "从Redis缓存读取IP黑名单时发生错误"

**解决**：
1. 系统自动降级到内存缓存，服务不中断
2. 检查 Redis 连接配置
3. 修复 Redis 后无需重启，20分钟后自动恢复

### 问题：黑名单未生效

**解决步骤**：
1. 检查配置文件或环境变量是否正确
2. 调用 `/api/IpBlacklist/refresh` 强制刷新
3. 查看日志确认加载成功

### 问题：CIDR 格式错误

**错误提示**：`"无效的CIDR格式: xxx"`

**解决**：
- 确保格式为 `IP/掩码位数`
- 掩码位数范围：0-32
- 示例：`192.168.1.0/24` ✅  `192.168.1.0/33` ❌

---

## 🔒 安全建议

### 1. API 权限控制
- 确保只有管理员能访问管理接口
- 定期审计黑名单操作日志

### 2. 配置管理
- 使用环境变量而非配置文件存储敏感IP
- 避免将黑名单配置提交到版本控制

### 3. 监控告警
- 设置黑名单命中率阈值告警
- 监控缓存命中率异常

---

## 📝 示例场景

### 场景 1：封禁单个恶意IP
```bash
# 1. 检测到恶意IP
GET /api/IpBlacklist/check/203.0.113.42
# 结果：isBlacklisted = false

# 2. 添加到黑名单
POST /api/IpBlacklist/add
{
  "ip": "203.0.113.42",
  "reason": "SQL注入攻击"
}

# 3. 验证封禁
GET /api/IpBlacklist/check/203.0.113.42
# 结果：isBlacklisted = true
```

### 场景 2：封禁整个网段
```bash
# 发现来自某个网段的DDoS攻击
POST /api/IpBlacklist/add
{
  "ip": "198.51.100.0/24",
  "reason": "DDoS攻击源"
}

# 该网段内所有IP (198.51.100.0 - 198.51.100.255) 都会被拦截
```

### 场景 3：解除误封
```bash
# 1. 用户反馈误封
POST /api/IpBlacklist/remove
{
  "ip": "192.0.2.15",
  "reason": "误报，已核实为正常用户"
}

# 2. 验证解封
GET /api/IpBlacklist/check/192.0.2.15
# 结果：isBlacklisted = false
```

---

## 🎓 高级用法

### 集成到自动化系统

```csharp
// 示例：自动封禁异常IP
public class AutoBanService
{
    private readonly IIpBlacklistCacheService _blacklistService;

    public async Task BanSuspiciousIpAsync(string ip)
    {
        // 检查IP行为
        if (await IsSuspicious(ip))
        {
            await _blacklistService.AddToBlacklistAsync(ip);
            _logger.LogWarning("自动封禁可疑IP: {Ip}", ip);
        }
    }
}
```

### 定时刷新缓存

```csharp
// 使用后台服务定时刷新
public class BlacklistRefreshHostedService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _blacklistService.RefreshBlacklistAsync();
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
```

---

## 📞 支持

如有问题，请查看：
- 日志文件：`logs/log-{date}.txt`
- 代码位置：[IpBlacklistCacheService.cs](rearend/iOSClub.WebAPI/iOSClub.WebAPI/Common/Security/IpBlacklistCacheService.cs)
- API 控制器：[IpBlacklistController.cs](rearend/iOSClub.WebAPI/iOSClub.WebAPI/Controllers/IpBlacklistController.cs)
