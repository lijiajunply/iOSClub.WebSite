using System.ComponentModel.DataAnnotations;

namespace iOSClub.Data.DataModels;

/// <summary>
/// 第三方应用客户端模型
/// </summary>
public class ClientApplication
{
    /// <summary>
    /// 客户端ID（主键）
    /// </summary>
    [Key]
    [MaxLength(50)]
    public string ClientId { get; init; } = "";

    /// <summary>
    /// 客户端密钥
    /// </summary>
    [MaxLength(100)]
    public string ClientSecret { get; set; } = "";

    /// <summary>
    /// 应用名称
    /// </summary>
    [MaxLength(100)]
    public string ApplicationName { get; set; } = "";

    /// <summary>
    /// 应用描述
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = "";

    /// <summary>
    /// 应用主页URL
    /// </summary>
    [MaxLength(200)]
    public string HomepageUrl { get; set; } = "";

    /// <summary>
    /// 回调URL白名单（多个URL用分号分隔）
    /// </summary>
    [MaxLength(1000)]
    public string RedirectUris { get; set; } = "";

    /// <summary>
    /// 应用图标URL
    /// </summary>
    [MaxLength(200)]
    public string LogoUrl { get; set; } = "";

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 是否支持PKCE
    /// </summary>
    public bool SupportsPkce { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 是否需要邮箱验证
    /// </summary>
    public bool IsNeedEMail { get; set; }

    /// <summary>
    /// 验证回调URL是否在白名单中
    /// </summary>
    /// <param name="redirectUri">要验证的回调URL</param>
    /// <returns>是否在白名单中</returns>
    public bool IsRedirectUriValid(string redirectUri)
    {
        if (string.IsNullOrEmpty(RedirectUris) || string.IsNullOrEmpty(redirectUri))
            return false;

        var uris = RedirectUris.Split(';', StringSplitOptions.RemoveEmptyEntries);
        return uris.Any(uri => string.Equals(uri.Trim(), redirectUri, StringComparison.OrdinalIgnoreCase));
    }
}