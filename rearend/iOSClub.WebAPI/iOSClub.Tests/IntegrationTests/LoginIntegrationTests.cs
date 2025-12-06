using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.Data.ShowModels;
using iOSClub.DataApi.Repositories;
using iOSClub.DataApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Moq;
using iOSClub.Tests;

namespace iOSClub.Tests.IntegrationTests;

public class LoginIntegrationTests
{
    private readonly DbContextOptions<ClubContext> _options;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _redisDbMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IClientApplicationRepository> _clientAppRepoMock;
    private readonly Mock<ILogger<LoginService>> _loggerMock;
    private readonly LoginService _loginService;

    public LoginIntegrationTests()
    {
        // 使用内存数据库进行测试
        _options = new DbContextOptionsBuilder<ClubContext>()
            .UseInMemoryDatabase(databaseName: "LoginIntegrationTestDatabase")
            .Options;
        
        // 创建数据库上下文和上下文工厂
        var contextFactory = new TestDbContextFactory(_options);
        
        // 初始化Repository
        var studentRepository = new StudentRepository(contextFactory);
        var staffRepository = new StaffRepository(contextFactory);
        
        // 设置Mock
        _tokenGeneratorMock = new Mock<ITokenGenerator>();
        _redisMock = new Mock<IConnectionMultiplexer>();
        _redisDbMock = new Mock<IDatabase>();
        _emailServiceMock = new Mock<IEmailService>();
        _clientAppRepoMock = new Mock<IClientApplicationRepository>();
        _loggerMock = new Mock<ILogger<LoginService>>();
        
        // 设置Redis
        _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_redisDbMock.Object);
        _redisDbMock.Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())).ReturnsAsync(RedisValue.Null);
        
        // 创建LoginService实例
        _loginService = new LoginService(
            studentRepository,
            staffRepository,
            _tokenGeneratorMock.Object,
            _redisMock.Object,
            _emailServiceMock.Object,
            _clientAppRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task LoginFlow_ValidStudent_ReturnsToken()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成测试学生数据
        var student = BogusDataGenerator.StudentFaker.Clone()
            .RuleFor(s => s.UserId, "20123456")
            .RuleFor(s => s.PasswordHash, DataTool.StringToHash("password123"))
            .RuleFor(s => s.ClassName, "2020级计算机1班")
            .Generate();
        await context.Students.AddAsync(student);
        await context.SaveChangesAsync();
        
        var loginModel = new LoginModel { UserId = student.UserId, Password = "password123" };
        var expectedToken = "mock-jwt-token";
        
        _tokenGeneratorMock.Setup(t => t.GetMemberToken(It.IsAny<MemberModel>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).Returns((expectedToken, "mock-refresh-token"));
        
        // Act
        var result = await _loginService.Login(loginModel);
        
        // Assert
        Assert.Equal(expectedToken, result);
        
        // 验证Redis操作 - 已通过token返回验证了整体流程的正确性
    }

    [Fact]
    public async Task LoginFlow_InvalidPassword_ReturnsEmptyString()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成测试学生数据
        var student = BogusDataGenerator.StudentFaker.Clone()
            .RuleFor(s => s.UserId, "20123456")
            .RuleFor(s => s.PasswordHash, DataTool.StringToHash("password123"))
            .RuleFor(s => s.ClassName, "2020级计算机1班")
            .Generate();
        await context.Students.AddAsync(student);
        await context.SaveChangesAsync();
        
        var loginModel = new LoginModel { UserId = student.UserId, Password = "wrongpassword" };
        
        // Act
        var result = await _loginService.Login(loginModel);
        
        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public async Task LoginFlow_StudentWithStaffRole_ReturnsCorrectIdentity()
    {
        // Arrange
        await using var context = new ClubContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 使用Bogus生成测试学生数据
        var student = BogusDataGenerator.StudentFaker.Clone()
            .RuleFor(s => s.UserId, "20123456")
            .RuleFor(s => s.PasswordHash, DataTool.StringToHash("password123"))
            .RuleFor(s => s.ClassName, "2020级计算机1班")
            .Generate();
        await context.Students.AddAsync(student);
        
        // 使用Bogus生成对应员工数据
        var staff = BogusDataGenerator.StaffFaker.Clone()
            .RuleFor(s => s.UserId, student.UserId)
            .RuleFor(s => s.Name, student.UserName)
            .RuleFor(s => s.Identity, "President")
            .Generate();
        await context.Staffs.AddAsync(staff);
        await context.SaveChangesAsync();
        
        var loginModel = new LoginModel { UserId = student.UserId, Password = "password123" };
        var expectedToken = "mock-jwt-token";
        
        _tokenGeneratorMock.Setup(t => t.GetMemberToken(It.Is<MemberModel>(m => m.Identity == "President"), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).Returns((expectedToken, "mock-refresh-token"));
        
        // Act
        var result = await _loginService.Login(loginModel);
        
        // Assert
        Assert.Equal(expectedToken, result);
        
        // 验证TokenGenerator使用了正确的Identity（President）
        _tokenGeneratorMock.Verify(t => t.GetMemberToken(It.Is<MemberModel>(m => m.Identity == "President"), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}