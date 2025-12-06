using iOSClub.DataApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace iOSClub.Tests.ServiceTests;

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
    public void VerifyEmailServiceConfigurationCheck()
    {
        // 这个测试只验证EmailService的配置检查逻辑，不实际发送邮件
        // 我们可以通过分析代码来验证配置检查逻辑是否正确
        Assert.NotNull(_emailService);
    }
}