namespace iOSClub.Data.VOs;

/// <summary>
/// 客户端应用视图对象 - 不含 ClientSecret
/// </summary>
public class ClientAppVO
{
    public string ClientId { get; set; } = "";
    public string ApplicationName { get; set; } = "";
    public string Description { get; set; } = "";
    public string HomepageUrl { get; set; } = "";
    public string RedirectUris { get; set; } = "";
    public string LogoUrl { get; set; } = "";
    public bool IsActive { get; set; }
    public bool SupportsPkce { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsNeedEMail { get; set; }
}
