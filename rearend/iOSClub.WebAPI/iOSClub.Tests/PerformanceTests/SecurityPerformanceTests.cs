using BenchmarkDotNet.Attributes;
using iOSClub.WebAPI.Common.Security;
using iOSClub.WebAPI.Common.Config;
using Microsoft.Extensions.Logging;
using Moq;
using iOSClub.Data.ShowModels;
using System.IO;
using System.Security.Cryptography;

namespace iOSClub.Tests.PerformanceTests;

/// <summary>
/// 安全服务性能测试
/// </summary>
[MemoryDiagnoser]
[HtmlExporter]
[CsvExporter]
public class SecurityPerformanceTests
{
    private JwtService _jwtService = null!;
    private RsaKeyManager _rsaKeyManager = null!;
    private JwtConfig _jwtConfig = null!;
    private Mock<ILogger<JwtService>> _loggerMock = null!;
    private Mock<ILogger<RsaKeyManager>> _rsaLoggerMock = null!;
    private string _testDirectory = null!;
    private MemberModel _testMember = null!;

    /// <summary>
    /// 测试设置
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        // 创建临时测试目录
        _testDirectory = Path.Combine(Path.GetTempPath(), "iOSClubPerformanceTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        
        // 设置JwtConfig
        _jwtConfig = new JwtConfig
        {
            RsaPrivateKeyPath = Path.Combine(_testDirectory, "test_private_key.pem"),
            RsaPublicKeyPath = Path.Combine(_testDirectory, "test_public_key.pem"),
            KeyRotationDays = 90,
            AccessTokenExpiryMinutes = 60,
            Issuer = "test-issuer",
            Audience = "test-audience"
        };
        
        // 设置Logger Mocks
        _loggerMock = new Mock<ILogger<JwtService>>();
        _rsaLoggerMock = new Mock<ILogger<RsaKeyManager>>();
        
        // 创建RsaKeyManager和JwtService实例
        _rsaKeyManager = new RsaKeyManager(_jwtConfig, _rsaLoggerMock.Object);
        _jwtService = new JwtService(_jwtConfig, _rsaKeyManager, _loggerMock.Object);
        
        // 创建测试用户模型
        _testMember = new MemberModel
        {
            UserId = "test_user_123",
            UserName = "Test User",
            Identity = "Student"
        };
    }

    /// <summary>
    /// 测试清理
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        // 删除临时测试目录
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    /// <summary>
    /// 基准测试：JWT令牌生成性能
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("JWT")]
    public void GenerateTokensPerformance()
    {
        _jwtService.GenerateTokens(_testMember);
    }

    /// <summary>
    /// 基准测试：JWT令牌生成性能（带权限范围和客户端ID）
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("JWT")]
    public void GenerateTokensWithScopeAndClientIdPerformance()
    {
        _jwtService.GenerateTokens(_testMember, "profile email role", "test-client-123");
    }

    /// <summary>
    /// 基准测试：JWT令牌验证性能
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("JWT")]
    public void ValidateAccessTokenPerformance()
    {
        var tokens = _jwtService.GenerateTokens(_testMember);
        _jwtService.ValidateAccessToken(tokens.AccessToken);
    }

    /// <summary>
    /// 基准测试：从JWT令牌提取声明性能
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("JWT")]
    public void ExtractClaimsFromTokenPerformance()
    {
        var tokens = _jwtService.GenerateTokens(_testMember);
        _jwtService.ExtractClaimsFromToken(tokens.AccessToken);
    }

    /// <summary>
    /// 基准测试：RSA密钥生成性能
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("RSA")]
    public void GenerateRsaKeyPairPerformance()
    {
        _rsaKeyManager.GenerateKeyPair();
    }

    /// <summary>
    /// 基准测试：RSA密钥存储性能
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("RSA")]
    public void StoreRsaKeyPairPerformance()
    {
        var rsa = _rsaKeyManager.GenerateKeyPair();
        _rsaKeyManager.StoreKeyPair(rsa, _jwtConfig.RsaPrivateKeyPath, _jwtConfig.RsaPublicKeyPath);
    }

    /// <summary>
    /// 基准测试：RSA密钥加载性能（私钥）
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("RSA")]
    public void LoadRsaPrivateKeyPerformance()
    {
        _rsaKeyManager.GetCurrentPrivateKey();
    }

    /// <summary>
    /// 基准测试：RSA密钥加载性能（公钥）
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("RSA")]
    public void LoadRsaPublicKeyPerformance()
    {
        _rsaKeyManager.GetCurrentPublicKey();
    }

    /// <summary>
    /// 基准测试：JWT刷新令牌性能
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("JWT")]
    public void RefreshAccessTokenPerformance()
    {
        var tokens = _jwtService.GenerateTokens(_testMember);
        _jwtService.RefreshAccessToken(tokens.RefreshToken, _testMember);
    }

    /// <summary>
    /// 基准测试：RSA密钥轮换性能
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("RSA")]
    public void RotateRsaKeysPerformance()
    {
        _rsaKeyManager.RotateKeys();
    }
}
