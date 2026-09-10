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

    /// <summary>
    /// 回调地址列表。与 ClientAppResultVO.RedirectUris 保持同一类型，
    /// 避免同一个 JSON key（redirectUris）出现字符串/数组两种形状。
    /// </summary>
    public List<string> RedirectUris { get; set; } = [];
    public string LogoUrl { get; set; } = "";
    public bool IsActive { get; set; }
    public bool SupportsPkce { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsNeedEMail { get; set; }
}
