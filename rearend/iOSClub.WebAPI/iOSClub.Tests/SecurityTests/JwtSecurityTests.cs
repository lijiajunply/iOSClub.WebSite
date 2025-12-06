using iOSClub.Data.ShowModels;
using iOSClub.WebAPI.Common.Config;
using iOSClub.WebAPI.Common.Security;
using Microsoft.Extensions.Logging;

namespace iOSClub.Tests.SecurityTests;

public class JwtSecurityTests
{
    private readonly ILogger<JwtService> _logger;
    private readonly JwtConfig _jwtConfig;
    private readonly ILogger<RsaKeyManager> _rsaLogger;

    public JwtSecurityTests()
    {
        // 配置日志记录器
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<JwtService>();
        _rsaLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<RsaKeyManager>();

        // 配置JWT设置，使用临时文件存储密钥
        var tempDir = Path.Combine(Path.GetTempPath(), "iOSClubTests");
        Directory.CreateDirectory(tempDir);

        _jwtConfig = new JwtConfig
        {
            AccessTokenExpiryMinutes = 20,
            RefreshTokenExpiryHours = 72,
            RsaPrivateKeyPath = Path.Combine(tempDir, "test_private_key.pem"),
            RsaPublicKeyPath = Path.Combine(tempDir, "test_public_key.pem"),
            Issuer = "Test Issuer",
            Audience = "Test Audience",
            KeyRotationDays = 90
        };
    }

    [Fact]
    public void TestRsaKeyGeneration()
    {
        // 测试RSA密钥生成
        var rsaKeyManager = new RsaKeyManager(_jwtConfig, _rsaLogger);

        // 生成密钥对
        var rsa = rsaKeyManager.GenerateKeyPair(2048);

        // 验证密钥大小
        Assert.Equal(2048, rsa.KeySize);

        // 验证密钥格式
        var privateKey = rsa.ExportPkcs8PrivateKeyPem();
        var publicKey = rsa.ExportRSAPublicKeyPem();

        Assert.StartsWith("-----BEGIN PRIVATE KEY-----", privateKey);
        Assert.StartsWith("-----BEGIN RSA PUBLIC KEY-----", publicKey);
    }

    [Fact]
    public void TestRsaKeyStorage()
    {
        // 测试RSA密钥存储
        var rsaKeyManager = new RsaKeyManager(_jwtConfig, _rsaLogger);

        // 生成并存储密钥对
        var rsa = rsaKeyManager.GenerateKeyPair(2048);
        rsaKeyManager.StoreKeyPair(rsa, _jwtConfig.RsaPrivateKeyPath, _jwtConfig.RsaPublicKeyPath);

        // 验证文件存在
        Assert.True(File.Exists(_jwtConfig.RsaPrivateKeyPath));
        Assert.True(File.Exists(_jwtConfig.RsaPublicKeyPath));

        // 测试从文件加载密钥
        var loadedPrivateKey = rsaKeyManager.LoadPrivateKey(_jwtConfig.RsaPrivateKeyPath);
        var loadedPublicKey = rsaKeyManager.LoadPublicKey(_jwtConfig.RsaPublicKeyPath);

        Assert.NotNull(loadedPrivateKey);
        Assert.NotNull(loadedPublicKey);

        Assert.Equal(2048, loadedPrivateKey.KeySize);
        Assert.Equal(2048, loadedPublicKey.KeySize);
    }

    [Fact]
    public void TestJwtTokenGeneration()
    {
        // 测试JWT令牌生成
        var rsaKeyManager = new RsaKeyManager(_jwtConfig, _rsaLogger);
        var jwtService = new JwtService(_jwtConfig, rsaKeyManager, _logger);

        // 生成密钥对
        var rsa = rsaKeyManager.GenerateKeyPair(2048);
        rsaKeyManager.StoreKeyPair(rsa, _jwtConfig.RsaPrivateKeyPath, _jwtConfig.RsaPublicKeyPath);

        // 创建测试用户模型
        var memberModel = new MemberModel
        {
            UserId = "test_user_123",
            UserName = "Test User",
            Identity = "Member"
        };

        // 生成令牌
        var (accessToken, refreshToken) = jwtService.GenerateTokens(memberModel, "full", "test_client");

        // 验证令牌格式
        Assert.NotNull(accessToken);
        Assert.NotNull(refreshToken);
        Assert.NotEmpty(accessToken);
        Assert.NotEmpty(refreshToken);

        // 验证访问令牌包含三个部分
        Assert.Equal(3, accessToken.Split('.').Length);

        // 验证刷新令牌格式
        Assert.Equal(32, refreshToken.Length); // GUID格式的刷新令牌
    }

    [Fact]
    public void TestJwtTokenValidation()
    {
        // 测试JWT令牌验证
        var rsaKeyManager = new RsaKeyManager(_jwtConfig, _rsaLogger);
        var jwtService = new JwtService(_jwtConfig, rsaKeyManager, _logger);

        // 生成密钥对
        var rsa = rsaKeyManager.GenerateKeyPair(2048);
        rsaKeyManager.StoreKeyPair(rsa, _jwtConfig.RsaPrivateKeyPath, _jwtConfig.RsaPublicKeyPath);

        // 创建测试用户模型
        var memberModel = new MemberModel
        {
            UserId = "test_user_123",
            UserName = "Test User",
            Identity = "Member"
        };

        // 生成令牌
        var (accessToken, _) = jwtService.GenerateTokens(memberModel, "full", "test_client");

        // 验证令牌
        var (isValid, claims) = jwtService.ValidateAccessToken(accessToken);

        // 验证令牌有效
        Assert.True(isValid);
    }
}
