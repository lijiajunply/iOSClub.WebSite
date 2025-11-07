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
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

#region 控制器基本设置

builder.Services.AddControllers();
builder.Services.AddOpenApi(opt => { opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

#endregion

#region 身份验证

builder.Services.AddAuthorizationCore();
// 只有在配置了 OAuth2 的情况下才添加 OAuth2 认证
// var oAuthConfig = builder.Configuration.GetSection("OAuth2");
// var clientId = oAuthConfig["ClientId"];

var authenticationBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "OAuth2"; // 总是使用OAuth2作为默认挑战方案
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true, //是否验证Issuer
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "iOSClub",
            ValidateAudience = true, //是否验证Audience
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "iOSClub",
            ValidateIssuerSigningKey = true, //是否验证SecurityKey

            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)), //SecurityKey
            ValidateLifetime = true, //是否验证失效时间
            ClockSkew = TimeSpan.FromSeconds(30), //过期时间容错值，解决服务器端时间不同步问题（秒）
            RequireExpirationTime = true,
        };
    });

// 配置我们自己的OAuth2认证系统
// 由于我们是OAuth提供商，我们需要提供登录页面和回调处理
// 这里我们使用Cookie认证作为基础，因为我们自己处理登录流程
authenticationBuilder.AddCookie("OAuth2", options =>
{
    options.LoginPath = "/oauth-login"; // 指向我们的OAuth登录页面
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
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
        policy.WithOrigins("https://ios.zeabur.app", "http://localhost:5173", "https://*.xauat.site/", "http://localhost:3000") // 允许指定来源
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // 如果需要发送凭据（如cookies、认证头等）
    });

    // 备用策略：如果需要允许多个特定来源
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("https://ios.zeabur.app", "https://localhost:3000", "http://localhost:3000")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

#endregion

#region 数据库设置

var sql = Environment.GetEnvironmentVariable("SQL", EnvironmentVariableTarget.Process);

if (string.IsNullOrEmpty(sql) && builder.Environment.IsDevelopment())
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

#region 仓库和服务的依赖注入

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
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