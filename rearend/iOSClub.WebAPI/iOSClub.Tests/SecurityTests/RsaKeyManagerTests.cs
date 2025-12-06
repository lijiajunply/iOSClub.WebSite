using iOSClub.WebAPI.Common.Security;
using Xunit;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using iOSClub.WebAPI.Common.Config;
using Microsoft.Extensions.Logging;
using Moq;

namespace iOSClub.Tests.SecurityTests;

public class RsaKeyManagerTests : IDisposable
{
    private readonly Mock<ILogger<RsaKeyManager>> _loggerMock;
    private readonly JwtConfig _jwtConfig;
    private readonly RsaKeyManager _rsaKeyManager;
    private readonly string _testDirectory;

    public RsaKeyManagerTests()
    {
        // 创建临时测试目录
        _testDirectory = Path.Combine(Path.GetTempPath(), "iOSClubTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        
        // 设置JwtConfig
        _jwtConfig = new JwtConfig
        {
            RsaPrivateKeyPath = Path.Combine(_testDirectory, "test_private_key.pem"),
            RsaPublicKeyPath = Path.Combine(_testDirectory, "test_public_key.pem"),
            KeyRotationDays = 90
        };
        
        // 设置Logger Mock
        _loggerMock = new Mock<ILogger<RsaKeyManager>>();
        
        // 创建RsaKeyManager实例
        _rsaKeyManager = new RsaKeyManager(_jwtConfig, _loggerMock.Object);
    }
    
    [Fact]
    public void GenerateKeyPair_ReturnsValidKeys()
    {
        // Act
        var rsa = _rsaKeyManager.GenerateKeyPair();
        
        // Assert
        Assert.NotNull(rsa);
        Assert.Equal(2048, rsa.KeySize);
        
        // 验证密钥可以导出
        var privateKey = rsa.ExportPkcs8PrivateKeyPem();
        var publicKey = rsa.ExportRSAPublicKeyPem();
        
        Assert.NotNull(privateKey);
        Assert.NotNull(publicKey);
        Assert.NotEmpty(privateKey);
        Assert.NotEmpty(publicKey);
        
        // 验证导出的密钥可以重新导入
        var importedRsa = RSA.Create();
        importedRsa.ImportFromPem(privateKey);
        Assert.Equal(2048, importedRsa.KeySize);
    }
    
    [Fact]
    public void GenerateKeyPair_WithDifferentKeySize_WorksCorrectly()
    {
        // Act
        var rsa = _rsaKeyManager.GenerateKeyPair(4096);
        
        // Assert
        Assert.NotNull(rsa);
        Assert.Equal(4096, rsa.KeySize);
    }
    
    [Fact]
    public void StoreKeyPair_WorksCorrectly()
    {
        // Arrange
        var rsa = RSA.Create(2048);
        
        // Act
        _rsaKeyManager.StoreKeyPair(rsa, _jwtConfig.RsaPrivateKeyPath, _jwtConfig.RsaPublicKeyPath);
        
        // Assert
        Assert.True(File.Exists(_jwtConfig.RsaPrivateKeyPath));
        Assert.True(File.Exists(_jwtConfig.RsaPublicKeyPath));
        
        // 验证文件内容有效
        var privateKeyContent = File.ReadAllText(_jwtConfig.RsaPrivateKeyPath);
        var publicKeyContent = File.ReadAllText(_jwtConfig.RsaPublicKeyPath);
        
        Assert.Contains("-----BEGIN PRIVATE KEY-----", privateKeyContent);
        Assert.Contains("-----END PRIVATE KEY-----", privateKeyContent);
        Assert.Contains("-----BEGIN RSA PUBLIC KEY-----", publicKeyContent);
        Assert.Contains("-----END RSA PUBLIC KEY-----", publicKeyContent);
    }
    
    [Fact]
    public void LoadPrivateKey_WorksCorrectly()
    {
        // Arrange - 生成并存储密钥
        var rsa = RSA.Create(2048);
        _rsaKeyManager.StoreKeyPair(rsa, _jwtConfig.RsaPrivateKeyPath, _jwtConfig.RsaPublicKeyPath);
        
        // Act
        var loadedRsa = _rsaKeyManager.LoadPrivateKey(_jwtConfig.RsaPrivateKeyPath);
        
        // Assert
        Assert.NotNull(loadedRsa);
        Assert.Equal(2048, loadedRsa.KeySize);
    }
    
    [Fact]
    public void LoadPublicKey_WorksCorrectly()
    {
        // Arrange - 生成并存储密钥
        var rsa = RSA.Create(2048);
        _rsaKeyManager.StoreKeyPair(rsa, _jwtConfig.RsaPrivateKeyPath, _jwtConfig.RsaPublicKeyPath);
        
        // Act
        var loadedRsa = _rsaKeyManager.LoadPublicKey(_jwtConfig.RsaPublicKeyPath);
        
        // Assert
        Assert.NotNull(loadedRsa);
        Assert.Equal(2048, loadedRsa.KeySize);
    }
    
    [Fact]
    public void LoadPrivateKey_ThrowsFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange - 使用不存在的文件路径
        var nonExistentPath = Path.Combine(_testDirectory, "non_existent_key.pem");
        
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => _rsaKeyManager.LoadPrivateKey(nonExistentPath));
    }
    
    [Fact]
    public void LoadPublicKey_ThrowsFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange - 使用不存在的文件路径
        var nonExistentPath = Path.Combine(_testDirectory, "non_existent_key.pem");
        
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => _rsaKeyManager.LoadPublicKey(nonExistentPath));
    }
    
    [Fact]
    public void IsKeyRotationNeeded_ReturnsTrue_WhenKeyDoesNotExist()
    {
        // Arrange - 使用不存在的文件路径
        var nonExistentPath = Path.Combine(_testDirectory, "non_existent_key.pem");
        
        // Act
        var result = _rsaKeyManager.IsKeyRotationNeeded(nonExistentPath);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsKeyRotationNeeded_ReturnsFalse_WhenKeyIsNew()
    {
        // Arrange - 生成并存储密钥
        var rsa = RSA.Create(2048);
        _rsaKeyManager.StoreKeyPair(rsa, _jwtConfig.RsaPrivateKeyPath, _jwtConfig.RsaPublicKeyPath);
        
        // Act
        var result = _rsaKeyManager.IsKeyRotationNeeded(_jwtConfig.RsaPrivateKeyPath);
        
        // Assert
        Assert.False(result, "Newly generated key should not need rotation");
    }
    
    [Fact]
    public void EnsureKeysValid_CreatesKeys_WhenTheyDoNotExist()
    {
        // Arrange - 确保密钥文件不存在
        if (File.Exists(_jwtConfig.RsaPrivateKeyPath))
        {
            File.Delete(_jwtConfig.RsaPrivateKeyPath);
        }
        if (File.Exists(_jwtConfig.RsaPublicKeyPath))
        {
            File.Delete(_jwtConfig.RsaPublicKeyPath);
        }
        
        // Act
        _rsaKeyManager.EnsureKeysValid();
        
        // Assert
        Assert.True(File.Exists(_jwtConfig.RsaPrivateKeyPath), "Private key file should be created");
        Assert.True(File.Exists(_jwtConfig.RsaPublicKeyPath), "Public key file should be created");
    }
    
    [Fact]
    public void GetCurrentPrivateKey_ReturnsValidKey()
    {
        // Act
        var rsa = _rsaKeyManager.GetCurrentPrivateKey();
        
        // Assert
        Assert.NotNull(rsa);
        Assert.Equal(2048, rsa.KeySize);
        
        // 验证密钥可以用于加密解密
        var testData = Encoding.UTF8.GetBytes("test data");
        var encryptedData = rsa.Encrypt(testData, RSAEncryptionPadding.OaepSHA256);
        Assert.NotNull(encryptedData);
    }
    
    [Fact]
    public void GetCurrentPublicKey_ReturnsValidKey()
    {
        // Act
        var rsa = _rsaKeyManager.GetCurrentPublicKey();
        
        // Assert
        Assert.NotNull(rsa);
        Assert.Equal(2048, rsa.KeySize);
    }
    
    [Fact]
    public void RotateKeys_WorksCorrectly()
    {
        // Arrange - 生成初始密钥
        _rsaKeyManager.EnsureKeysValid();
        var initialPrivateKey = File.ReadAllText(_jwtConfig.RsaPrivateKeyPath);
        var initialPublicKey = File.ReadAllText(_jwtConfig.RsaPublicKeyPath);
        
        // Act - 执行密钥轮换
        _rsaKeyManager.RotateKeys();
        
        // Assert - 验证新密钥已生成
        var newPrivateKey = File.ReadAllText(_jwtConfig.RsaPrivateKeyPath);
        var newPublicKey = File.ReadAllText(_jwtConfig.RsaPublicKeyPath);
        
        Assert.NotEqual(initialPrivateKey, newPrivateKey);
        Assert.NotEqual(initialPublicKey, newPublicKey);
        
        // 验证旧密钥已备份
        var backupFiles = Directory.GetFiles(_testDirectory, "*.bak");
        Assert.True(backupFiles.Length >= 2, "Backup files should be created");
    }
    
    [Fact]
    public void StoreKeyPair_OverwritesExistingFiles()
    {
        // Arrange - 生成并存储初始密钥
        var initialRsa = RSA.Create(2048);
        _rsaKeyManager.StoreKeyPair(initialRsa, _jwtConfig.RsaPrivateKeyPath, _jwtConfig.RsaPublicKeyPath);
        var initialPrivateKey = File.ReadAllText(_jwtConfig.RsaPrivateKeyPath);
        
        // Act - 生成并存储新密钥
        var newRsa = RSA.Create(2048);
        _rsaKeyManager.StoreKeyPair(newRsa, _jwtConfig.RsaPrivateKeyPath, _jwtConfig.RsaPublicKeyPath);
        var newPrivateKey = File.ReadAllText(_jwtConfig.RsaPrivateKeyPath);
        
        // Assert - 验证文件已被覆盖
        Assert.NotEqual(initialPrivateKey, newPrivateKey);
    }
    
    // 清理临时文件
    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }
}
