using System.Text;
using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.DataApi.Services;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using NpgsqlDataProtection;
using Scalar.AspNetCore;
using StackExchange.Redis;
using System.Security.Claims;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

#region 控制器基本设置

builder.Services.AddControllers();
builder.Services.AddOpenApi(opt => { opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

#endregion

#region 身份验证

builder.Services.AddAuthorizationCore();
// 只有在配置了 OAuth2 的情况下才添加 OAuth2 认证
var oAuthConfig = builder.Configuration.GetSection("OAuth2");
var clientId = oAuthConfig["ClientId"];

var authenticationBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        // 只有配置了 OAuth2 时才设置 DefaultChallengeScheme
        if (!string.IsNullOrEmpty(clientId))
        {
            options.DefaultChallengeScheme = "OAuth2";
        }
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true, //是否验证Issuer
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "iOSClub",
            ValidateAudience = true, //是否验证Audience
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "iOSClubApp",
            ValidateIssuerSigningKey = true, //是否验证SecurityKey

            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)), //SecurityKey
            ValidateLifetime = true, //是否验证失效时间
            ClockSkew = TimeSpan.FromSeconds(30), //过期时间容错值，解决服务器端时间不同步问题（秒）
            RequireExpirationTime = true,
        };
    });

// 只有在配置了 OAuth2 的情况下才添加 OAuth2 认证
if (!string.IsNullOrEmpty(clientId))
{
    authenticationBuilder.AddOAuth("OAuth2", options =>
    {
        options.ClientId = clientId;
        options.ClientSecret = oAuthConfig["ClientSecret"] ?? "";
        options.CallbackPath = oAuthConfig["CallbackPath"] ?? "/auth/callback";

        options.AuthorizationEndpoint = oAuthConfig["AuthorizationEndpoint"] ?? "";
        options.TokenEndpoint = oAuthConfig["TokenEndpoint"] ?? "";
        options.UserInformationEndpoint = oAuthConfig["UserInformationEndpoint"] ?? "";

        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                if (!response.IsSuccessStatusCode)
                    return;

                var json = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<JsonElement>(json);
                context.RunClaimActions(user);
            }
        };
    });
}

#endregion

#region 跨域设置

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://ios.zeabur.app", "http://localhost:5173") // 允许指定来源
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

app.UseHttpsRedirection();
app.UseAuthentication(); // 添加这行以启用身份验证中间件
app.UseAuthorization();
app.UseCors();
app.MapControllers();
app.MapScalarApiReference();

app.Run();