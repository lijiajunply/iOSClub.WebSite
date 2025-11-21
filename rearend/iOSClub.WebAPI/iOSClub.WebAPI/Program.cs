using System.Data.SQLite;
using System.Text;
using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.DataApi.Services;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using NpgsqlDataProtection;
using Scalar.AspNetCore;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

#region 控制器基本设置

builder.Services.AddControllers(options => { options.Filters.Add<GlobalAuthorizationFilter>(); });
builder.Services.AddOpenApi(opt => { opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

#endregion

#region 身份验证

builder.Services.AddAuthorizationCore();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var secretKey = Environment.GetEnvironmentVariable("SECRETKEY", EnvironmentVariableTarget.Process) ??
                        builder.Configuration["Jwt:SecretKey"] ?? "";
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = "iOS Club of XAUAT",
            ValidateAudience = true,
            ValidAudience = "iOS Club of XAUAT",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(10),
            RequireExpirationTime = true,
        };
    })
    .AddCookie("OAuth2", options =>
    {
        options.LoginPath = "/OAuth/login";
        options.LogoutPath = "/OAuth/logout";
        options.AccessDeniedPath = "/OAuth/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
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

// 添加内存缓存支持，解决会话存储需要IDistributedCache的问题
builder.Services.AddDistributedMemoryCache();

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
            .AllowCredentials(); // 如果需要发送凭据（如cookies、认证头等）
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
string sqlPath = "";

if (builder.Environment.IsProduction())
{
    sqlPath = Environment.CurrentDirectory + "/logs/log.db";

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
builder.Services.AddScoped<TokenActionFilter>();
builder.Services.AddScoped<IJwtHelper, JwtHelper>();

builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
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


var app = builder.Build();

// 注册日志清理后台服务 (仅在生产环境且日志路径已设置时启用)
if (builder.Environment.IsProduction() && !string.IsNullOrEmpty(sqlPath))
{
    app.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStarted.Register(() =>
    {
        Task.Run(async () =>
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("日志清理服务已启动，将定期清理7天前的日志");

            while (true)
            {
                try
                {
                    // 等待一天后执行清理
                    await Task.Delay(TimeSpan.FromDays(1));

                    // 清理7天前的日志
                    using var connection = new SQLiteConnection($"Data Source={sqlPath}");
                    await connection.OpenAsync();
                    using var command = new SQLiteCommand("DELETE FROM Logs WHERE Timestamp < @cutoffDate", connection);
                    command.Parameters.AddWithValue("@cutoffDate", DateTime.Now.AddDays(-7));
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            logger.LogInformation("清理了 {RowsAffected} 条7天前的日志", rowsAffected);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "清理旧日志时出错");
                }
            }
        });
    });
}

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

    if (!context.Staffs.Any())
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

    if (context.Departments.Any())
    {
        var departments = await context.Departments.Where(x => string.IsNullOrEmpty(x.Key)).ToListAsync();
        foreach (var department in departments)
        {
            department.Key = department.GetHashKey();
        }
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