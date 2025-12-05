using Serilog.Core;
using Serilog.Events;
using System.Text.RegularExpressions;

namespace iOSClub.WebAPI.Common.Security;

/// <summary>
/// 敏感数据过滤器，用于过滤日志中的敏感信息
/// </summary>
public class SensitiveDataFilter : ILogEventEnricher
{
    private static readonly List<Regex> SensitivePatterns = new List<Regex>
    {
        // 密码模式
        new Regex(@"(?i)(password|pwd|passwd)\s*[:=]\s*(['""`]?)(.*?)\2", RegexOptions.Compiled),
        // JWT令牌模式
        new Regex(@"(?i)(token|access_token|refresh_token|jwt|bearer)\s*[:=]\s*(['""`]?)([a-zA-Z0-9_\-\.]+)\2", RegexOptions.Compiled),
        // API密钥模式
        new Regex(@"(?i)(api_key|api_secret|api_token|secret_key|access_key|secret)\s*[:=]\s*(['""`]?)(.*?)\2", RegexOptions.Compiled),
        // 手机号模式（中国）
        new Regex(@"1[3-9]\d{9}", RegexOptions.Compiled),
        // 身份证号模式（中国）
        new Regex(@"[1-9]\d{5}(18|19|20)\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{3}[\dXx]", RegexOptions.Compiled),
        // 银行卡号模式（简单匹配）
        new Regex(@"\d{16,19}", RegexOptions.Compiled),
        // 邮箱模式
        new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled)
    };

    /// <summary>
    /// 过滤敏感数据
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <returns>过滤后的字符串</returns>
    public static string FilterSensitiveData(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = input;
        foreach (var pattern in SensitivePatterns)
        {
            result = pattern.Replace(result, match =>
            {
                // 根据匹配的模式返回不同的脱敏结果
                if (match.Groups.Count > 3 && !string.IsNullOrEmpty(match.Groups[1].Value))
                {
                    // 对于键值对类型的敏感数据，保留键名，脱敏值
                    var key = match.Groups[1].Value;
                    var quote = match.Groups[2].Value;
                    return $"{key}={quote}[REDACTED]{quote}";
                }
                
                // 对于纯敏感数据（如手机号、身份证号），直接返回脱敏结果
                if (match.Value.Length == 11) // 手机号
                    return $"{match.Value.Substring(0, 3)}****{match.Value.Substring(7)}";
                
                if (match.Value.Length == 18) // 身份证号
                    return $"{match.Value.Substring(0, 6)}***********{match.Value.Substring(17)}";
                
                if (match.Value.Length >= 16 && match.Value.Length <= 19) // 银行卡号
                    return $"{match.Value.Substring(0, 4)}********{match.Value.Substring(match.Value.Length - 4)}";
                
                // 邮箱脱敏
                if (match.Value.Contains('@'))
                {
                    var parts = match.Value.Split('@');
                    if (parts[0].Length > 2)
                        return $"{parts[0].Substring(0, 2)}****@{parts[1]}";
                    return $"{parts[0][0]}****@{parts[1]}";
                }
                
                return "[REDACTED]";
            });
        }
        
        return result;
    }

    /// <summary>
    /// 丰富日志事件，过滤敏感数据
    /// </summary>
    /// <param name="logEvent">日志事件</param>
    /// <param name="propertyFactory">属性工厂</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // 过滤消息模板
        if (logEvent.MessageTemplate != null)
        {
            var originalMessage = logEvent.MessageTemplate.Text;
            var filteredMessage = FilterSensitiveData(originalMessage);
            if (originalMessage != filteredMessage)
            {
                logEvent.AddOrUpdateProperty(new LogEventProperty("OriginalMessage", new ScalarValue(originalMessage)));
                logEvent.AddOrUpdateProperty(new LogEventProperty("FilteredMessage", new ScalarValue(filteredMessage)));
            }
        }
        
        // 过滤日志属性
        foreach (var property in logEvent.Properties.ToList())
        {
            if (property.Value is ScalarValue scalarValue && scalarValue.Value is string stringValue)
            {
                var filteredValue = FilterSensitiveData(stringValue);
                if (stringValue != filteredValue)
                {
                    logEvent.AddOrUpdateProperty(new LogEventProperty(property.Key, new ScalarValue(filteredValue)));
                }
            }
        }
    }
}

/// <summary>
/// 日志审计服务，用于定期检查日志内容
/// </summary>
public class LogAuditService
{
    private readonly ILogger<LogAuditService> _logger;
    
    public LogAuditService(ILogger<LogAuditService> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// 审计日志内容，检查是否包含未过滤的敏感信息
    /// </summary>
    /// <param name="logContent">日志内容</param>
    public void AuditLog(string logContent)
    {
        // 检查是否包含未过滤的敏感信息
        var sensitivePatterns = new List<Regex>
        {
            new Regex(@"(?i)(password|pwd|passwd)\s*[:=]\s*(['""`]?)(?!\[REDACTED\])(.*?)\2", RegexOptions.Compiled),
            new Regex(@"(?i)(token|access_token|refresh_token|jwt|bearer)\s*[:=]\s*(['""`]?)(?!\[REDACTED\])([a-zA-Z0-9_\-\.]+)\2", RegexOptions.Compiled),
            new Regex(@"(?i)(api_key|api_secret|api_token|secret_key|access_key|secret)\s*[:=]\s*(['""`]?)(?!\[REDACTED\])(.*?)\2", RegexOptions.Compiled)
        };
        
        foreach (var pattern in sensitivePatterns)
        {
            if (pattern.IsMatch(logContent))
            {
                _logger.LogWarning("发现未过滤的敏感信息: {LogContent}", logContent.Substring(0, Math.Min(200, logContent.Length)));
                break;
            }
        }
    }
}
