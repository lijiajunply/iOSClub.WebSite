using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace iOSClub.DataApi.Services;

public interface IEmailService
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="to">收件人邮箱地址</param>
    /// <param name="subject">邮件主题</param>
    /// <param name="body">邮件正文</param>
    /// <param name="isHtml">是否为HTML格式</param>
    /// <returns>发送结果</returns>
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false);
}

public class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        var smtpServer = configuration["Email:SmtpServer"];
        var port = int.Parse(configuration["Email:Port"] ?? "587");
        var username = configuration["Email:Username"];
        var password = configuration["Email:Password"];
        var fromAddress = configuration["Email:FromAddress"];

        // 检查必要配置是否存在
        if (string.IsNullOrEmpty(smtpServer) ||
            string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(fromAddress))
        {
            return false;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("", fromAddress));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            if (isHtml)
            {
                bodyBuilder.HtmlBody = body;
            }
            else
            {
                bodyBuilder.TextBody = body;
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            // 根据端口选择安全选项
            var secureSocketOptions = port switch
            {
                587 => SecureSocketOptions.StartTls,
                465 => SecureSocketOptions.SslOnConnect,
                _ => SecureSocketOptions.Auto
            };

            await client.ConnectAsync(smtpServer, port, secureSocketOptions);
            await client.AuthenticateAsync(fromAddress, password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }
        catch
        {
            return false;
        }
    }
}