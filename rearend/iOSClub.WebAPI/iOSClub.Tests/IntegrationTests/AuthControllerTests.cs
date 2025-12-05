using iOSClub.Data; 
using iOSClub.Data.DataModels; 
using iOSClub.Data.ShowModels; 
using Microsoft.AspNetCore.Mvc.Testing; 
using Microsoft.Extensions.DependencyInjection; 
using Microsoft.EntityFrameworkCore; 
using Moq; 
using iOSClub.DataApi.Services; 
using iOSClub.DataApi.Repositories; 
using iOSClub.WebAPI.IdentityModels; 
using StackExchange.Redis; 
using System.Net.Http.Headers; 
using System.Text; 
using Newtonsoft.Json; 
using System.Net.Http.Json; 

namespace iOSClub.Tests.IntegrationTests; 

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>> 
{ 
    private readonly WebApplicationFactory<Program> _factory; 
    private readonly HttpClient _client; 
    private readonly DbContextOptions<ClubContext> _options; 
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock; 
    private readonly Mock<IConnectionMultiplexer> _redisMock; 
    private readonly Mock<IDatabase> _redisDbMock; 
    private readonly Mock<IEmailService> _emailServiceMock; 

    public AuthControllerTests(WebApplicationFactory<Program> factory) 
    { 
        _factory = factory; 
        
        // 使用内存数据库进行测试 
        _options = new DbContextOptionsBuilder<ClubContext>() 
            .UseInMemoryDatabase(databaseName: "AuthControllerTestDatabase") 
            .Options; 
        
        // 设置Mock 
        _tokenGeneratorMock = new Mock<ITokenGenerator>(); 
        _redisMock = new Mock<IConnectionMultiplexer>(); 
        _redisDbMock = new Mock<IDatabase>(); 
        _emailServiceMock = new Mock<IEmailService>(); 
        
        // 设置Redis 
        _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_redisDbMock.Object); 
        _redisDbMock.Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())).ReturnsAsync(RedisValue.Null); 
        
        // 创建HttpClient并配置服务 
        _client = _factory.WithWebHostBuilder(builder => 
        { 
            builder.ConfigureServices(services => 
            { 
                // 替换ClubContext为内存数据库版本 
                var descriptor = services.SingleOrDefault( 
                    d => d.ServiceType == typeof(DbContextOptions<ClubContext>)); 
                if (descriptor != null) 
                { 
                    services.Remove(descriptor); 
                } 
                services.AddDbContext<ClubContext>(options => 
                { 
                    options.UseInMemoryDatabase("AuthControllerTestDatabase"); 
                }); 
                
                // 替换其他服务为Mock版本 
                services.AddSingleton(_tokenGeneratorMock.Object); 
                services.AddSingleton(_redisMock.Object); 
                services.AddSingleton(_emailServiceMock.Object); 
                
                // 注册Repository 
                services.AddScoped<IStudentRepository, StudentRepository>(); 
                services.AddScoped<IStaffRepository, StaffRepository>(); 
            }); 
        }).CreateClient(); 
    } 
    
    [Fact] 
    public async Task Register_Success_ReturnsToken() 
    { 
        // Arrange 
        await using var context = new ClubContext(_options); 
        await context.Database.EnsureDeletedAsync(); 
        await context.Database.EnsureCreatedAsync(); 
        
        var studentModel = new StudentModel 
        { 
            UserId = "20210001", 
            UserName = "Test Student", 
            PasswordHash = DataTool.StringToHash("password123"), 
            Academy = "Computer Science", 
            Gender = "男", 
            PoliticalLandscape = "共青团员", 
            ClassName = "2021级1班", 
            PhoneNum = "13800138000" 
        }; 
        
        var expectedToken = "mock-jwt-token"; 
        _tokenGeneratorMock.Setup(t => t.GetMemberToken(It.IsAny<MemberModel>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).Returns((expectedToken, "mock-refresh-token")); 
        
        // Act 
        var response = await _client.PostAsJsonAsync("/Auth/signup", studentModel); 
        
        // Assert 
        response.EnsureSuccessStatusCode(); // 200 OK 
        var content = await response.Content.ReadAsStringAsync(); 
        var result = JsonConvert.DeserializeObject<ApiResponse<string>>(content); 
        
        Assert.NotNull(result); 
        Assert.True(result.Code == 200); 
        Assert.Equal(expectedToken, result.Data); 
        Assert.Equal("注册成功", result.Message); 
    } 
    
    [Fact] 
    public async Task Login_StudentSuccess_ReturnsToken() 
    { 
        // Arrange 
        await using var context = new ClubContext(_options); 
        await context.Database.EnsureDeletedAsync(); 
        await context.Database.EnsureCreatedAsync(); 
        
        // 添加测试学生数据 
        var student = new StudentModel 
        { 
            UserId = "20210001", 
            UserName = "Test Student", 
            PasswordHash = DataTool.StringToHash("password123"), 
            Academy = "Computer Science", 
            Gender = "男", 
            PoliticalLandscape = "共青团员", 
            ClassName = "2021级1班", 
            PhoneNum = "13800138000" 
        }; 
        await context.Students.AddAsync(student); 
        await context.SaveChangesAsync(); 
        
        var loginModel = new LoginModel { UserId = student.UserId, Password = "password123" }; 
        var expectedToken = "mock-jwt-token"; 
        
        _tokenGeneratorMock.Setup(t => t.GetMemberToken(It.IsAny<MemberModel>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).Returns((expectedToken, "mock-refresh-token")); 
        _redisDbMock.Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())).ReturnsAsync(RedisValue.Null); 
        
        // Act 
        var response = await _client.PostAsJsonAsync("/Auth/login?clientId=test-client", loginModel); 
        
        // Assert 
        response.EnsureSuccessStatusCode(); // 200 OK 
        var content = await response.Content.ReadAsStringAsync(); 
        var result = JsonConvert.DeserializeObject<ApiResponse<string>>(content); 
        
        Assert.NotNull(result); 
        Assert.True(result.Code == 200); 
        Assert.Equal(expectedToken, result.Data); 
        Assert.Equal("登录成功", result.Message); 
    } 
    
    [Fact] 
    public async Task Login_InvalidPassword_ReturnsError() 
    { 
        // Arrange 
        await using var context = new ClubContext(_options); 
        await context.Database.EnsureDeletedAsync(); 
        await context.Database.EnsureCreatedAsync(); 
        
        // 添加测试学生数据 
        var student = new StudentModel 
        { 
            UserId = "20210001", 
            UserName = "Test Student", 
            PasswordHash = DataTool.StringToHash("password123"), 
            Academy = "Computer Science", 
            Gender = "男", 
            PoliticalLandscape = "共青团员", 
            ClassName = "2021级1班", 
            PhoneNum = "13800138000" 
        }; 
        await context.Students.AddAsync(student); 
        await context.SaveChangesAsync(); 
        
        var loginModel = new LoginModel { UserId = student.UserId, Password = "wrongpassword" }; 
        
        // Act 
        var response = await _client.PostAsJsonAsync("/Auth/login", loginModel); 
        
        // Assert 
        response.EnsureSuccessStatusCode(); // 200 OK，但返回错误信息 
        var content = await response.Content.ReadAsStringAsync(); 
        var result = JsonConvert.DeserializeObject<ApiResponse<string>>(content); 
        
        Assert.NotNull(result); 
        Assert.True(result.Code != 200); 
        Assert.Equal("用户不存在或密码错误", result.Message); 
    } 
    
    [Fact] 
    public async Task Login_UserNotFound_ReturnsError() 
    { 
        // Arrange 
        await using var context = new ClubContext(_options); 
        await context.Database.EnsureDeletedAsync(); 
        await context.Database.EnsureCreatedAsync(); 
        
        // 不添加任何用户数据 
        var loginModel = new LoginModel { UserId = "non_existent_user", Password = "password123" }; 
        
        // Act 
        var response = await _client.PostAsJsonAsync("/Auth/login", loginModel); 
        
        // Assert 
        response.EnsureSuccessStatusCode(); // 200 OK，但返回错误信息 
        var content = await response.Content.ReadAsStringAsync(); 
        var result = JsonConvert.DeserializeObject<ApiResponse<string>>(content); 
        
        Assert.NotNull(result); 
        Assert.True(result.Code != 200); 
        Assert.Equal("用户不存在或密码错误", result.Message); 
    } 
    
    [Fact] 
    public async Task Logout_Success_ReturnsTrue() 
    { 
        // Arrange 
        await using var context = new ClubContext(_options); 
        await context.Database.EnsureDeletedAsync(); 
        await context.Database.EnsureCreatedAsync(); 
        
        // 添加测试学生数据 
        var student = new StudentModel 
        { 
            UserId = "20210001", 
            UserName = "Test Student", 
            PasswordHash = DataTool.StringToHash("password123"), 
            Academy = "Computer Science", 
            Gender = "男", 
            PoliticalLandscape = "共青团员", 
            ClassName = "2021级1班", 
            PhoneNum = "13800138000" 
        }; 
        await context.Students.AddAsync(student); 
        await context.SaveChangesAsync(); 
        
        var loginModel = new LoginModel { UserId = student.UserId, Password = "password123" }; 
        var token = "mock-jwt-token"; 
        
        _tokenGeneratorMock.Setup(t => t.GetMemberToken(It.IsAny<MemberModel>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).Returns((token, "mock-refresh-token")); 
        _redisDbMock.Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())).ReturnsAsync(RedisValue.Null); 
        _redisDbMock.Setup(r => r.KeyDeleteAsync(It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>())).ReturnsAsync(2L); 
        
        // 先登录获取token 
        var loginResponse = await _client.PostAsJsonAsync("/Auth/login", loginModel); 
        loginResponse.EnsureSuccessStatusCode(); 
        
        // 设置Authorization头 
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); 
        
        // Act 
        var response = await _client.PostAsync($"/Auth/logout?userId={student.UserId}", null); 
        
        // Assert 
        response.EnsureSuccessStatusCode(); // 200 OK 
        var content = await response.Content.ReadAsStringAsync(); 
        var result = JsonConvert.DeserializeObject<ApiResponse<bool>>(content); 
        
        Assert.NotNull(result); 
        Assert.True(result.Code == 200); 
        Assert.True(result.Data); 
        Assert.Equal("登出成功", result.Message); 
        
        // 清除Authorization头 
        _client.DefaultRequestHeaders.Authorization = null; 
    } 
    
    [Fact] 
    public async Task RefreshToken_ValidRefreshToken_ReturnsNewToken() 
    { 
        // Arrange 
        await using var context = new ClubContext(_options); 
        await context.Database.EnsureDeletedAsync(); 
        await context.Database.EnsureCreatedAsync(); 
        
        var userId = "20210001"; 
        var refreshToken = "valid-refresh-token"; 
        var newAccessToken = "new-mock-jwt-token"; 
        
        _redisDbMock.Setup(r => r.StringGetAsync(It.Is<RedisKey>(k => k.ToString() == $"refresh:{userId}"), It.IsAny<CommandFlags>())).ReturnsAsync(refreshToken); 
        _tokenGeneratorMock.Setup(t => t.GetMemberToken(It.IsAny<MemberModel>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).Returns((newAccessToken, refreshToken)); 
        
        // Act 
        var response = await _client.PostAsync($"/Auth/refresh-token?userId={userId}&refreshToken={refreshToken}", null); 
        
        // Assert 
        response.EnsureSuccessStatusCode(); // 200 OK 
        var content = await response.Content.ReadAsStringAsync(); 
        var result = JsonConvert.DeserializeObject<ApiResponse<string>>(content); 
        
        Assert.NotNull(result); 
        Assert.True(result.Code == 200); 
        Assert.Equal(newAccessToken, result.Data); 
        Assert.Equal("刷新令牌成功", result.Message); 
    } 
    
    [Fact] 
    public async Task RefreshToken_InvalidRefreshToken_ReturnsError() 
    { 
        // Arrange 
        var userId = "20210001"; 
        var invalidRefreshToken = "invalid-refresh-token"; 
        
        _redisDbMock.Setup(r => r.StringGetAsync(It.Is<RedisKey>(k => k.ToString() == $"refresh:{userId}"), It.IsAny<CommandFlags>())).ReturnsAsync(RedisValue.Null); 
        
        // Act 
        var response = await _client.PostAsync($"/Auth/refresh-token?userId={userId}&refreshToken={invalidRefreshToken}", null); 
        
        // Assert 
        response.EnsureSuccessStatusCode(); // 200 OK，但返回错误信息 
        var content = await response.Content.ReadAsStringAsync(); 
        var result = JsonConvert.DeserializeObject<ApiResponse<string>>(content); 
        
        Assert.NotNull(result); 
        Assert.True(result.Code != 200); 
        Assert.Equal("无效的刷新令牌", result.Message); 
    } 
    
    [Fact] 
    public async Task RefreshToken_MissingParameters_ReturnsError() 
    { 
        // Act - 缺少userId和refreshToken参数 
        var response = await _client.PostAsync("/Auth/refresh-token", null); 
        
        // Assert 
        response.EnsureSuccessStatusCode(); // 200 OK，但返回错误信息 
        var content = await response.Content.ReadAsStringAsync(); 
        var result = JsonConvert.DeserializeObject<ApiResponse<string>>(content); 
        
        Assert.NotNull(result); 
        Assert.True(result.Code != 200); 
        Assert.Equal("用户ID和刷新令牌不能为空", result.Message); 
    } 
} 

// 定义ApiResponse类以匹配控制器返回的响应格式 
public class ApiResponse<T> 
{ 
    public int Code { get; set; } 
    public int ErrorCode { get; set; } 
    public string? Message { get; set; } 
    public T? Data { get; set; } 
    
    public static ApiResponse<T> Success(T data, string message = "Success") 
    { 
        return new ApiResponse<T> { Code = 200, ErrorCode = 0, Message = message, Data = data }; 
    } 
    
    public static ApiResponse<T> Fail(int errorCode, string message = "Fail") 
    { 
        return new ApiResponse<T> { Code = errorCode, ErrorCode = errorCode, Message = message, Data = default }; 
    } 
} 
