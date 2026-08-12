using System.ComponentModel.DataAnnotations;

namespace iOSClub.Data.DTOs;

public class ClientAppCreateDTO
{
    [Required]
    public string ApplicationName { get; set; } = "";
    public string Description { get; set; } = "";
    public string HomepageUrl { get; set; } = "";
    public List<string> RedirectUris { get; set; } = [];
    public string LogoUrl { get; set; } = "";
    public bool IsNeedEMail { get; set; }
    public bool SupportsPkce { get; set; }
}
