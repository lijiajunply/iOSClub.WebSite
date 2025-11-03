using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

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
        try
        {
            var smtpServer = configuration["Email:SmtpServer"];
            var port = int.Parse(configuration["Email:Port"] ?? "587");
            var username = configuration["Email:Username"];
            var password = configuration["Email:Password"];
            var fromAddress = configuration["Email:FromAddress"];
            var fromName = configuration["Email:FromName"] ?? "iOS Club";
            var enableSsl = bool.Parse(configuration["Email:EnableSsl"] ?? "true");

            // 检查必要配置是否存在
            if (string.IsNullOrEmpty(smtpServer) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(fromAddress))
            {
                return false;
            }

            using var client = new SmtpClient(smtpServer, port);
            client.EnableSsl = enableSsl;
            client.Credentials = new NetworkCredential(username, password);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            return true;
        }
        catch (Exception)
        {
            // 在实际应用中，应该记录日志
            return false;
        }
    }
}