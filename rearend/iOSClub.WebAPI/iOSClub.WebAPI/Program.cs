using System.Data;
using System.IO.Compression;
using System.Security.Cryptography;
using FluentValidation.AspNetCore;
using iOSClub.Data;
using iOSClub.Data.DataObjects;
using iOSClub.Data.Mappers;
using iOSClub.DataApi.Services;
using iOSClub.WebAPI.Common;
using iOSClub.WebAPI.Common.Config;
using iOSClub.WebAPI.Common.Extensions;
using iOSClub.WebAPI.Common.Middleware;
using iOSClub.WebAPI.Common.Security;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using NpgsqlDataProtection;
using ParadeDB.EntityFrameworkCore.Extensions;
using Prometheus;
using Scalar.AspNetCore;
using Serilog;
using StackExchange.Redis;
using DotEnv.Core;

new EnvLoader().Load();

var builder = WebApplication.CreateBuilder(args);

// 配置Mapster全局映射
MapperConfig.Configure();

#region 控制器基本设置

// 配置请求大小限制
builder.Services.Configure<FormOptions>(options =>
{
    // 设置请求体大小上限为10MB
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
    options.ValueLengthLimit = 10 * 1024 * 1024;
    options.BufferBodyLengthLimit = 10 * 1024 * 1024;
});

// 配置Kestrel服务器的请求大小限制
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});

// 注册FluentValidation服务（使用旧版API，抑制警告）
#pragma warning disable CS0618
var mvcBuilder = builder.Services.AddControllers(options => { options.Filters.Add<GlobalAuthorizationFilter>(); });
mvcBuilder.AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssemblyContaining<Program>();
    fv.DisableDataAnnotationsValidation = false;
});
#pragma warning restore CS0618

builder.Services.AddOpenApi(opt => { opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

#endregion

#region JWT配置和密钥管理

var jwtConfig = new JwtConfig
{
    AccessTokenExpiryMinutes =
        int.TryParse(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRY_MINUTES"), out var accessTokenExpiry)
            ? accessTokenExpiry
            : 20,
    RefreshTokenExpiryHours =
        int.TryParse(Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRY_HOURS"), out var refreshTokenExpiry)
            ? refreshTokenExpiry
            : 72,
    RsaPrivateKeyPath = Environment.GetEnvironmentVariable("JWT_RSA_PRIVATE_KEY_PATH") ?? "./app/keys/rsa_private.pem",
    RsaPublicKeyPath = Environment.GetEnvironmentVariable("JWT_RSA_PUBLIC_KEY_PATH") ?? "./app/keys/rsa_public.pem",
    Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "iOS Club of XAUAT",
    Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "iOS Club of XAUAT",
    KeyRotationDays = int.TryParse(Environment.GetEnvironmentVariable("JWT_KEY_ROTATION_DAYS"), out var keyRotationDays)
        ? keyRotationDays
        : 90
};

builder.Services.AddSingleton(jwtConfig);
builder.Services.AddSingleton<RsaKeyManager>();
builder.Services.AddSingleton<JwtService>();

#endregion

#region 身份验证

builder.Services.AddAuthorizationCore();

// 配置JWT认证 - 注意：在测试环境中，我们将使用服务注入的方式获取RsaKeyManager，
// 而不是直接创建实例，这样可以允许测试代码替换该服务
var rsaKeyManager = new RsaKeyManager(jwtConfig,
    LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole()).CreateLogger<RsaKeyManager>());

// 尝试确保密钥有效，但如果失败（例如在测试环境中），我们将使用临时密钥
RSAParameters rsaParams;
try
{
    rsaKeyManager.EnsureKeysValid();
    var publicKey = rsaKeyManager.GetCurrentPublicKey();
    rsaParams = publicKey.ExportParameters(false);
}
catch (Exception)
{
    // 在测试环境或无法访问文件系统的环境中，生成临时密钥
    using var rsa = RSA.Create(2048);
    rsaParams = rsa.ExportParameters(false);
}

// 从RSA密钥中导出公钥的SHA256哈希值作为KeyId，与生成令牌时使用的KeyId保持一致
var publicKeyBytes = RSA.Create(rsaParams).ExportRSAPublicKey();
var keyId = Convert.ToBase64String(SHA256.HashData(publicKeyBytes)).Substring(0, 16);
var rsaSecurityKey = new RsaSecurityKey(rsaParams) { KeyId = keyId };

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = rsaSecurityKey,
            ValidateIssuer = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtConfig.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            RequireExpirationTime = true,
            RequireSignedTokens = true
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                // 可以在这里添加额外的验证逻辑
                context.Success();
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                // 记录认证失败日志
                context.NoResult();
                context.Fail("认证失败");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
        };
    })
    .AddCookie("OAuth2", options =>
    {
        options.LoginPath = "/OAuth/login";
        options.LogoutPath = "/OAuth/logout";
        options.AccessDeniedPath = "/OAuth/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.None;
    });

#endregion

#region 会话支持

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None; // 允许跨站点发送Cookie
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // 根据请求类型决定是否使用安全Cookie
});

// 使用Redis分布式缓存
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redis = Environment.GetEnvironmentVariable("REDIS", EnvironmentVariableTarget.Process);
    if (string.IsNullOrEmpty(redis) && builder.Environment.IsDevelopment())
    {
        redis = builder.Configuration["Redis"];
    }

    if (!string.IsNullOrEmpty(redis))
    {
        options.Configuration = redis;
        // 设置实例名称为空，避免key前缀导致与IConnectionMultiplexer不一致
        options.InstanceName = null;
    }
});

// 添加内存缓存（用于本地缓存层）
builder.Services.AddMemoryCache();

#endregion

#region 跨域设置

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin =>
                origin.EndsWith(".zeabur.app") || // 支持所有 zeabur.app 子域名
                origin.EndsWith(".xauat.site") || // 支持所有 xauat.site 子域名
                origin.StartsWith("http://localhost")) // 支持本地开发环境
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials() // 如果需要发送凭据（如cookies、认证头等）
            .WithExposedHeaders("X-Refresh-Token"); // 允许前端访问X-Refresh-Token响应头
    });
});

#endregion

#region 数据库设置

var sql = Environment.GetEnvironmentVariable("SQL", EnvironmentVariableTarget.Process);

if (string.IsNullOrEmpty(sql))
{
    sql = builder.Configuration["SQL"];
}

if (string.IsNullOrEmpty(sql))
{
    builder.Services.AddDbContextFactory<ClubContext>(opt =>
        opt.UseSqlite("Data Source=Data.db",
            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("./keys"));
}
else
{
    builder.Services.AddDbContextFactory<ClubContext>(opt =>
    {
        opt.UseNpgsql(sql,
            o =>
            {
                o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                o.UseParadeDb();
            });
        opt.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    });

    builder.Services.AddDataProtection()
        .PersistKeysToPostgres(sql, true);
}

var redis = Environment.GetEnvironmentVariable("REDIS", EnvironmentVariableTarget.Process);
if (string.IsNullOrEmpty(redis) && builder.Environment.IsDevelopment())
{
    redis = builder.Configuration["Redis"];
}

if (!string.IsNullOrEmpty(redis))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redis));
}

#endregion

#region 日志设置

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole();
}
else
{
    // 定义日志数据库路径
    var logPath = Path.Combine(Environment.CurrentDirectory, "logs", "log.db");

    // 确保日志目录存在
    var logDir = Path.GetDirectoryName(logPath);
    if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
    {
        Directory.CreateDirectory(logDir);
    }

    // 统一日志配置，适用于所有环境
    var logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .Enrich.With<SensitiveDataFilter>()
        .WriteTo.Console(
            outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.SQLite(
            sqliteDbPath: logPath,
            tableName: "Logs")
        .WriteTo.File(
            Path.Combine(logDir ?? Environment.CurrentDirectory, "log-.txt"),
            rollingInterval: RollingInterval.Day,
            outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}")
        .CreateLogger();

    builder.Logging
        .ClearProviders()
        .AddConsole()
        .AddDebug()
        .SetMinimumLevel(LogLevel.Information)
        .AddSerilog(logger);
}

#endregion

#region 仓库和服务的依赖注入

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GlobalAuthorizationFilter>();
// 注册ITokenGenerator服务
builder.Services.AddScoped<ITokenGenerator, JwtGenerator>();

// 使用扩展方法注册服务
builder.Services.RegisterRepositoriesAndServices();
builder.Services.RegisterSecurityServices();

#endregion

#region 压缩

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest; // 或 CompressionLevel.Optimal
});

builder.Services.Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.Fastest; });

#endregion

#region Prometheus 监控

// Prometheus 指标收集不需要 AddMetricServer
// 使用 UseHttpMetrics() 和 MapMetrics() 来配置指标端点

#endregion

var app = builder.Build();

// 注册全局异常处理中间件
app.UseMiddleware<GlobalExceptionMiddleware>();

// 注册请求频率限制中间件
app.UseMiddleware<RateLimitMiddleware>();

// 注册数据脱敏中间件
app.UseDataMasking();

// 配置安全响应头
app.Use(async (context, next) =>
{
    // 添加内容安全策略，防止XSS攻击
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; object-src 'none'; frame-ancestors 'none';");

    // 添加X-XSS-Protection头
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

    // 添加X-Content-Type-Options头，防止MIME嗅探
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

    // 添加X-Frame-Options头，防止点击劫持
    context.Response.Headers.Append("X-Frame-Options", "DENY");

    // 添加Referrer-Policy头
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    // 添加Permissions-Policy头
    context.Response.Headers.Append("Permissions-Policy",
        "camera=(), microphone=(), geolocation=(), payment=(), fullscreen=*");

    // 添加Strict-Transport-Security头（生产环境建议启用）
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }

    await next();
});

// 优化数据库迁移策略，异步执行迁移
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ClubContext>();

    try
    {
        var pending = context.Database.GetPendingMigrations();
        var enumerable = pending as string[] ?? pending.ToArray();

        if (enumerable.Length != 0)
        {
            Console.WriteLine("Pending migrations: " + string.Join("; ", enumerable));
            await context.Database.MigrateAsync();
            Console.WriteLine("Migrations applied successfully.");
        }
        else
        {
            Console.WriteLine("No pending migrations.");
        }

        // 数据迁移：信息与控制工程学院拆分 - 查看受影响学生数据
        // await DataMigration_InfoControlEngineering(context);

        // 初始化数据
        if (!await context.Staffs.AnyAsync())
        {
            var user = Environment.GetEnvironmentVariable("USER", EnvironmentVariableTarget.Process);
            Console.WriteLine(user);
            var model = new StaffDO() { Identity = "Founder", Name = "root", UserId = "0000000000" };
            var users = user?.Split(',');
            if (!string.IsNullOrEmpty(user) && users != null)
            {
                if (users.Length > 0)
                    model.Name = users[0];
                if (users.Length > 1)
                    model.UserId = users[1];
            }

            context.Staffs.Add(model);
        }

        if (await context.Categories.AnyAsync())
        {
            var categories = await context.Categories.Where(x => string.IsNullOrEmpty(x.Id)).ToListAsync();
            context.Categories.RemoveRange(categories);
        }

        await context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Migration error: " + ex);
        // 不要抛出异常，避免应用启动失败
    }
    finally
    {
        await context.DisposeAsync();
    }
}

// 别动
app.MapOpenApi();

// 先配置会话中间件
app.UseSession(); // 会话中间件应该在认证和跨域中间件之前

app.UseHttpsRedirection();
// CORS 必须在认证/授权之前运行，才能正确处理携带 Authorization 的跨域预检请求。
app.UseCors();
app.UseAuthentication(); // 添加这行以启用身份验证中间件
app.UseAuthorization();

// 添加 Prometheus HTTP 请求指标收集中间件
app.UseHttpMetrics();

app.MapControllers();
app.MapScalarApiReference();

// 暴露 Prometheus 指标端点
app.MapMetrics();

app.Run();

#region 数据迁移方法

/// <summary>
/// 数据迁移：信息与控制工程学院拆分
/// 按专业（ClassName）自动将23级及以后的学生拆分到两个新学院
/// </summary>
static async Task DataMigration_InfoControlEngineering(ClubContext context)
{
    try
    {
        // 仅在 PostgreSQL 环境下执行
        if (context.Database.ProviderName != "Npgsql.EntityFrameworkCore.PostgreSQL")
            return;

        // 注意：此连接由 EF Core 管理生命周期，不要 dispose
        var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        Console.WriteLine("[数据迁移] ========== 信息与控制工程学院拆分 ==========");

        // ========== 阶段0：诊断查询 ==========

        // 查询1：查看受影响的学生数（UserId >= '23'）
        await using var cmd1 = connection.CreateCommand();
        cmd1.CommandText = """
                           SELECT COUNT(*) AS affected_count
                           FROM "Students"
                           WHERE "Academy" = '信息与控制工程学院'
                             AND LEFT("UserId", 2) >= '23';
                           """;
        var affectedCount = await cmd1.ExecuteScalarAsync();
        Console.WriteLine($"[数据迁移] 受影响学生数 (UserId >= '23'): {affectedCount}");

        // 查询2：查看受影响学生的明细（按班级分组，辅助判断拆分规则）
        await using var cmd2 = connection.CreateCommand();
        cmd2.CommandText = """
                           SELECT LEFT("UserId", 2) AS grade, "ClassName", COUNT(*) AS cnt
                           FROM "Students"
                           WHERE "Academy" = '信息与控制工程学院'
                             AND LEFT("UserId", 2) >= '23'
                           GROUP BY LEFT("UserId", 2), "ClassName"
                           ORDER BY grade, cnt DESC;
                           """;
        await using var reader2 = await cmd2.ExecuteReaderAsync();
        Console.WriteLine("[数据迁移] 受影响学生明细 (按年级+班级分组):");
        while (await reader2.ReadAsync())
        {
            Console.WriteLine($"  年级: {reader2["grade"]}, 班级: {reader2["ClassName"]}, 人数: {reader2["cnt"]}");
        }
        await reader2.DisposeAsync();

        // 查询3：查看不受影响的学生（UserId < '23'，保留原学院）
        await using var cmd3 = connection.CreateCommand();
        cmd3.CommandText = """
                           SELECT COUNT(*) AS keep_count
                           FROM "Students"
                           WHERE "Academy" = '信息与控制工程学院'
                             AND LEFT("UserId", 2) < '23';
                           """;
        var keepCount = await cmd3.ExecuteScalarAsync();
        Console.WriteLine($"[数据迁移] 保留原学院学生数 (UserId < '23'): {keepCount}");

        if (affectedCount is 0L or 0)
        {
            Console.WriteLine("[数据迁移] 没有需要迁移的学生，跳过。");
            return;
        }

        // ========== 阶段1：干跑 - 查看班级归属 ==========
        await using var cmdDryRun = connection.CreateCommand();
        cmdDryRun.CommandText = """
                                SELECT "ClassName", COUNT(*) AS cnt,
                                       CASE
                                           WHEN "ClassName" LIKE '%人工智能%' OR "ClassName" LIKE '%自动化%'
                                                OR "ClassName" LIKE '%机器人工程%'
                                           THEN '人工智能与机器人学院'
                                           WHEN "ClassName" LIKE '%计算机科学与技术%' OR "ClassName" LIKE '%通信工程%'
                                           THEN '计算机和信息工程学院'
                                           ELSE '未匹配，保留不动'
                                       END AS target_academy
                                FROM "Students"
                                WHERE "Academy" = '信息与控制工程学院'
                                  AND LEFT("UserId", 2) >= '23'
                                GROUP BY "ClassName"
                                ORDER BY target_academy, cnt DESC;
                                """;
        await using var readerDryRun = await cmdDryRun.ExecuteReaderAsync();
        Console.WriteLine("[数据迁移] 班级归属映射 (干跑):");
        var unmatchedClasses = new List<string>();
        while (await readerDryRun.ReadAsync())
        {
            var className = readerDryRun["ClassName"].ToString()!;
            var cnt = readerDryRun["cnt"];
            var target = readerDryRun["target_academy"].ToString()!;
            Console.WriteLine($"  {className} (人数: {cnt}) {target}");
            if (target == "未匹配，保留不动")
                unmatchedClasses.Add(className);
        }
        await readerDryRun.DisposeAsync();

        if (unmatchedClasses.Count > 0)
        {
            Console.WriteLine($"[数据迁移] ⚠️ 警告：{unmatchedClasses.Count} 个班级未匹配到任何关键词，将被跳过:");
            foreach (var c in unmatchedClasses)
                Console.WriteLine($"  - {c}");
        }

        // ========== 阶段2：执行拆分 ==========

        // 1: 划分到「人工智能与机器人学院」
        //    人工智能、自动化、机器人工程 → 人工智能与机器人学院
        await using var cmdUpdate1 = connection.CreateCommand();
        cmdUpdate1.CommandText = """
                                 UPDATE "Students"
                                 SET "Academy" = '人工智能与机器人学院'
                                 WHERE "Academy" = '信息与控制工程学院'
                                   AND LEFT("UserId", 2) >= '23'
                                   AND (
                                     "ClassName" LIKE '%人工智能%'
                                      OR "ClassName" LIKE '%自动化%'
                                      OR "ClassName" LIKE '%机器人工程%'
                                      OR "ClassName" LIKE '%机器人%'
                                      OR "ClassName" LIKE '%人智%'
                                   );
                                 """;
        var update1Count = await cmdUpdate1.ExecuteNonQueryAsync();
        Console.WriteLine($"[数据迁移] 迁移到「人工智能与机器人学院」: {update1Count} 人");

        // 2: 划分到「计算机和信息工程学院」
        //    计算机科学与技术、通信工程 → 计算机和信息工程学院
        await using var cmdUpdate2 = connection.CreateCommand();
        cmdUpdate2.CommandText = """
                                 UPDATE "Students"
                                 SET "Academy" = '计算机和信息工程学院'
                                 WHERE "Academy" = '信息与控制工程学院'
                                   AND LEFT("UserId", 2) >= '23'
                                   AND (
                                     "ClassName" LIKE '%计算机科学与技术%'
                                      OR "ClassName" LIKE '%通信工程%'
                                      OR "ClassName" LIKE '%计算机%'
                                      OR "ClassName" LIKE '%计科%'
                                      OR "ClassName" LIKE '%通信%'
                                   );
                                 """;
        var update2Count = await cmdUpdate2.ExecuteNonQueryAsync();
        Console.WriteLine($"[数据迁移] 迁移到「计算机和信息工程学院」: {update2Count} 人");

        // ========== 阶段3：检查漏网之鱼 ==========
        await using var cmdLeak = connection.CreateCommand();
        cmdLeak.CommandText = """
                              SELECT "UserId", "UserName", "ClassName"
                              FROM "Students"
                              WHERE "Academy" = '信息与控制工程学院'
                                AND LEFT("UserId", 2) >= '23';
                              """;
        await using var readerLeak = await cmdLeak.ExecuteReaderAsync();
        var leakCount = 0;
        while (await readerLeak.ReadAsync())
        {
            if (leakCount == 0)
                Console.WriteLine("[数据迁移] ⚠️ 以下学生未被匹配，需人工处理:");
            leakCount++;
            Console.WriteLine($"  {readerLeak["UserId"]} | {readerLeak["UserName"]} | {readerLeak["ClassName"]}");
        }
        if (leakCount == 0)
            Console.WriteLine("[数据迁移] ✅ 无漏网之鱼，所有23级后学生已迁移完毕。");
        await readerLeak.DisposeAsync();

        // ========== 阶段4：迁移后验证 ==========
        await using var cmdVerify1 = connection.CreateCommand();
        cmdVerify1.CommandText = """
                                 SELECT COUNT(*) AS should_be_zero
                                 FROM "Students"
                                 WHERE "Academy" = '信息与控制工程学院'
                                   AND LEFT("UserId", 2) >= '23';
                                 """;
        var shouldBeZero = await cmdVerify1.ExecuteScalarAsync();
        Console.WriteLine($"[数据迁移] 验证 - 23级后仍保留在原学院的学生数 (应为0): {shouldBeZero}");

        await using var cmdVerify2 = connection.CreateCommand();
        cmdVerify2.CommandText = """
                                 SELECT COUNT(*) AS old_students_kept
                                 FROM "Students"
                                 WHERE "Academy" = '信息与控制工程学院'
                                   AND LEFT("UserId", 2) < '23';
                                 """;
        var oldKept = await cmdVerify2.ExecuteScalarAsync();
        Console.WriteLine($"[数据迁移] 验证 - 22级及以前保留在原学院的学生数: {oldKept}");

        Console.WriteLine("[数据迁移] ========== 拆分完成 ==========");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[数据迁移] 信息与控制工程学院拆分失败: {ex.Message}");
        Console.WriteLine($"[数据迁移] 详细错误: {ex}");
    }
}

#endregion
