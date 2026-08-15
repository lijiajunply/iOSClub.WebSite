namespace iOSClub.Data.VOs;

/// <summary>
/// 学生视图对象 - 不含密码哈希等敏感字段
/// </summary>
public class StudentVO
{
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Academy { get; set; } = "";
    public string PoliticalLandscape { get; set; } = "";
    public string Gender { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string PhoneNum { get; set; } = "";
    public DateTime JoinTime { get; set; }
    public string? EMail { get; set; }
}
