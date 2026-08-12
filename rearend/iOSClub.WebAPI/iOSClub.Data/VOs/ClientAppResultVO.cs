namespace iOSClub.Data.VOs;

/// <summary>
/// 客户端应用创建/密钥重置结果视图 - 含 ClientSecret（仅创建/重置时显示）
/// </summary>
public class ClientAppResultVO
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string ApplicationName { get; set; } = "";
    public string Description { get; set; } = "";
    public string HomepageUrl { get; set; } = "";
    public List<string> RedirectUris { get; set; } = [];
    public string LogoUrl { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public bool IsNeedEMail { get; set; }
    public bool SupportsPkce { get; set; }
}
