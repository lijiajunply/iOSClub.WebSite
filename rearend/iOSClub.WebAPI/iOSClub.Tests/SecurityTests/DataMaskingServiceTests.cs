using iOSClub.WebAPI.Common.Security;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace iOSClub.Tests.SecurityTests;

/// <summary>
/// 数据脱敏服务测试
/// </summary>
public class DataMaskingServiceTests
{
    private readonly DataMaskingService _maskingService;
    private readonly Mock<ILogger<DataMaskingService>> _loggerMock;
    
    public DataMaskingServiceTests()
    {
        _loggerMock = new Mock<ILogger<DataMaskingService>>();
        var config = new MaskingConfig();
        _maskingService = new DataMaskingService(config);
    }
    
    /// <summary>
    /// 测试手机号脱敏功能
    /// </summary>
    [Fact]
    public void ApplyMasking_ShouldMaskPhoneNumbers()
    {
        // Arrange
        var testCases = new List<(string input, string expectedOutput)>
        {
            ("13812345678", "138****5678"),
            ("13987654321", "139****4321"),
            ("15912345678", "159****5678"),
        };
        
        // Act & Assert
        foreach (var (input, expectedOutput) in testCases)
        {
            var result = _maskingService.ApplyMasking(input, MaskingType.PhoneNumber);
            Assert.Equal(expectedOutput, result);
        }
    }
    
    /// <summary>
    /// 测试身份证号脱敏功能
    /// </summary>
    [Fact]
    public void ApplyMasking_ShouldMaskIdCards()
    {
        // Arrange
        var testCases = new List<(string input, string expectedOutput)>
        {
            ("110101199001011234", "110101***********34"),
            ("440101200001012345", "440101***********45"),
            ("310101199505053456", "310101***********56"),
        };
        
        // Act & Assert
        foreach (var (input, expectedOutput) in testCases)
        {
            var result = _maskingService.ApplyMasking(input, MaskingType.IdCard);
            Assert.Equal(expectedOutput, result);
        }
    }
    
    /// <summary>
    /// 测试邮箱脱敏功能
    /// </summary>
    [Fact]
    public void ApplyMasking_ShouldMaskEmails()
    {
        // Arrange
        var testCases = new List<(string input, string expectedOutput)>
        {
            ("user@example.com", "u****@example.com"),
            ("test.user@test.com", "te****@test.com"),
            ("admin@example.org", "ad****@example.org"),
        };
        
        // Act & Assert
        foreach (var (input, expectedOutput) in testCases)
        {
            var result = _maskingService.ApplyMasking(input, MaskingType.Email);
            Assert.Equal(expectedOutput, result);
        }
    }
    
    /// <summary>
    /// 测试姓名脱敏功能
    /// </summary>
    [Fact]
    public void ApplyMasking_ShouldMaskNames()
    {
        // Arrange
        var testCases = new List<(string input, string expectedOutput)>
        {
            ("张三", "张*"),
            ("李四", "李*"),
            ("王五", "王*"),
            ("张三丰", "张**"),
        };
        
        // Act & Assert
        foreach (var (input, expectedOutput) in testCases)
        {
            var result = _maskingService.ApplyMasking(input, MaskingType.Name);
            Assert.Equal(expectedOutput, result);
        }
    }
    
    /// <summary>
    /// 测试密码脱敏功能
    /// </summary>
    [Fact]
    public void ApplyMasking_ShouldMaskPasswords()
    {
        // Arrange
        var testCases = new List<(string input, string expectedOutput)>
        {
            ("mysecretpassword", "******"),
            ("Test@123", "******"),
            ("", "******"),
        };
        
        // Act & Assert
        foreach (var (input, expectedOutput) in testCases)
        {
            var result = _maskingService.ApplyMasking(input, MaskingType.Password);
            Assert.Equal(expectedOutput, result);
        }
    }
    
    /// <summary>
    /// 测试银行卡号脱敏功能
    /// </summary>
    [Fact]
    public void ApplyMasking_ShouldMaskBankCards()
    {
        // Arrange
        var testCases = new List<(string input, string expectedOutput)>
        {
            ("6222021234567890123", "6222********0123"),
            ("5555555555554444", "5555********4444"),
            ("4111111111111111", "4111********1111"),
        };
        
        // Act & Assert
        foreach (var (input, expectedOutput) in testCases)
        {
            var result = _maskingService.ApplyMasking(input, MaskingType.BankCard);
            Assert.Equal(expectedOutput, result);
        }
    }
}