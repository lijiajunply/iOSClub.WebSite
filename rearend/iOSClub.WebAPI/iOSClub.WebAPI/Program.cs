using System.IO.Compression;
using System.Security.Cryptography;
using FluentValidation.AspNetCore;
using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.DataApi.Services;
using iOSClub.WebAPI.Common;
using iOSClub.WebAPI.Common.Config;
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
using Scalar.AspNetCore;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

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

// 配置JWT认证
var rsaKeyManager = new RsaKeyManager(jwtConfig,
    LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole()).CreateLogger<RsaKeyManager>());
rsaKeyManager.EnsureKeysValid();

var publicKey = rsaKeyManager.GetCurrentPublicKey();

// 从RSA密钥中导出公钥的SHA256哈希值作为KeyId，与生成令牌时使用的KeyId保持一致
var publicKeyBytes = publicKey.ExportRSAPublicKey();
var keyId = Convert.ToBase64String(SHA256.HashData(publicKeyBytes)).Substring(0, 16);
var rsaSecurityKey = new RsaSecurityKey(publicKey) { KeyId = keyId };

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

// 使用Redis分布式缓存替代内存缓存
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
    }
});

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
            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
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

// 定义日志数据库路径

if (builder.Environment.IsProduction())
{
    var sqlPath = Environment.CurrentDirectory + "/logs/log.db";

    // 确保日志目录存在
    var logDir = Path.GetDirectoryName(sqlPath);
    if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
    {
        Directory.CreateDirectory(logDir);
    }

    // 日志 注册
    var logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.SQLite(
            sqliteDbPath: sqlPath,
            tableName: "Logs")
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

builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

builder.Services.AddScoped<IDataCentreService, DataCentreService>();
builder.Services.AddScoped<IClientApplicationRepository, ClientApplicationRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILoginService, LoginService>();

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

var app = builder.Build();

// 注册全局异常处理中间件
app.UseMiddleware<GlobalExceptionMiddleware>();

// 注册请求频率限制中间件
app.UseMiddleware<RateLimitMiddleware>();

// 配置安全响应头
app.Use(async (context, next) =>
{
    // 添加内容安全策略，防止XSS攻击
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; object-src 'none'; frame-ancestors 'none';");
    
    // 添加X-XSS-Protection头
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    
    // 添加X-Content-Type-Options头，防止MIME嗅探
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    
    // 添加X-Frame-Options头，防止点击劫持
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    
    // 添加Referrer-Policy头
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // 添加Permissions-Policy头
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=(), fullscreen=*");
    
    // 添加Strict-Transport-Security头（生产环境建议启用）
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }
    
    await next();
});

// 创建数据库
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ClubContext>();

    var pending = context.Database.GetPendingMigrations();
    var enumerable = pending as string[] ?? pending.ToArray();

    if (enumerable.Length != 0)
    {
        Console.WriteLine("Pending migrations: " + string.Join("; ", enumerable));
        try
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("Migrations applied successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Migration error: " + ex);
            throw; // 让异常冒泡，方便定位问题
        }
    }
    else
    {
        Console.WriteLine("No pending migrations.");
    }

    if (!await context.Staffs.AnyAsync())
    {
        var user = Environment.GetEnvironmentVariable("USER", EnvironmentVariableTarget.Process);
        Console.WriteLine(user);
        var model = new StaffModel() { Identity = "Founder", Name = "root", UserId = "0000000000" };
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

    // if (context.Departments.Any())
    // {
    //     var departments = await context.Departments.Where(x => string.IsNullOrEmpty(x.Key)).ToListAsync();
    //     foreach (var department in departments)
    //     {
    //         department.Key = department.GetHashKey();
    //     }
    // }

    if (await context.Categories.AnyAsync())
    {
        var categories = await context.Categories.Where(x => string.IsNullOrEmpty(x.Id)).ToListAsync();
        context.Categories.RemoveRange(categories);
    }

    await context.SaveChangesAsync();
    await context.DisposeAsync();
}

// 别动
app.MapOpenApi();

// 先配置会话中间件
app.UseSession(); // 会话中间件应该在认证和跨域中间件之前

app.UseHttpsRedirection();
app.UseAuthentication(); // 添加这行以启用身份验证中间件
app.UseAuthorization();
app.UseCors();
app.MapControllers();
app.MapScalarApiReference();

app.Run();