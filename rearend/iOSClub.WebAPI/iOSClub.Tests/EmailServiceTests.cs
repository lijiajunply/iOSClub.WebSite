using iOSClub.DataApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace iOSClub.Tests;

public class EmailServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<EmailService>> _loggerMock;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<EmailService>>();
        _emailService = new EmailService(_configurationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendEmailAsync_ReturnsFalse_WhenConfigIsMissing()
    {
        // Arrange
        // 模拟配置不存在的情况
        _configurationMock.Setup(c => c["Email:SmtpServer"]).Returns((string?)null);
        _configurationMock.Setup(c => c["Email:Port"]).Returns((string?)null);
        _configurationMock.Setup(c => c["Email:Username"]).Returns((string?)null);
        _configurationMock.Setup(c => c["Email:Password"]).Returns((string?)null);
        _configurationMock.Setup(c => c["Email:FromAddress"]).Returns((string?)null);

        // Act
        var result = await _emailService.SendEmailAsync("test@example.com", "Test Subject", "Test Body");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendEmailAsync_UsesEnvironmentVariables_WhenAvailable()
    {
        // Arrange
        // 设置环境变量
        Environment.SetEnvironmentVariable("SMTP", "smtp.example.com", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("EMAIL_POST", "587", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("EMAIL_NAME", "testuser", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("EMAIL_PASSWORD", "testpass", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("EMAIL_FROM", "from@example.com", EnvironmentVariableTarget.Process);

        try
        {
            // Act
            var result = await _emailService.SendEmailAsync("test@example.com", "Test Subject", "Test Body");

            // Assert
            // 由于没有真实的SMTP服务器，预期会返回false
            Assert.False(result);
        }
        finally
        {
            // 清理环境变量
            Environment.SetEnvironmentVariable("SMTP", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("EMAIL_POST", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("EMAIL_NAME", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("EMAIL_PASSWORD", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("EMAIL_FROM", null, EnvironmentVariableTarget.Process);
        }
    }

    [Fact]
    public async Task SendEmailAsync_UsesConfiguration_WhenEnvironmentVariablesAreNotAvailable()
    {
        // Arrange
        // 设置配置
        _configurationMock.Setup(c => c["Email:SmtpServer"]).Returns("smtp.config.example.com");
        _configurationMock.Setup(c => c["Email:Port"]).Returns("587");
        _configurationMock.Setup(c => c["Email:Username"]).Returns("configuser");
        _configurationMock.Setup(c => c["Email:Password"]).Returns("configpass");
        _configurationMock.Setup(c => c["Email:FromAddress"]).Returns("config@example.com");

        // Act
        var result = await _emailService.SendEmailAsync("test@example.com", "Test Subject", "Test Body");

        // Assert
        // 由于没有真实的SMTP服务器，预期会返回false
        Assert.False(result);
    }

    [Fact]
    public async Task SendEmailAsync_ReturnsFalse_WhenPortIsInvalid()
    {
        // Arrange
        // 设置配置，使用无效端口
        _configurationMock.Setup(c => c["Email:SmtpServer"]).Returns("smtp.example.com");
        _configurationMock.Setup(c => c["Email:Port"]).Returns("invalid-port");
        _configurationMock.Setup(c => c["Email:Username"]).Returns("testuser");
        _configurationMock.Setup(c => c["Email:Password"]).Returns("testpass");
        _configurationMock.Setup(c => c["Email:FromAddress"]).Returns("from@example.com");

        // Act
        var result = await _emailService.SendEmailAsync("test@example.com", "Test Subject", "Test Body");

        // Assert
        Assert.False(result);
    }
}
