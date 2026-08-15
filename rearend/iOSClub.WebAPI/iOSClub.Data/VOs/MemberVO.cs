namespace iOSClub.Data.VOs;

/// <summary>
/// 成员视图对象 - 用于成员列表和JWT认证
/// 包含学生基本信息和身份，不含密码哈希
/// </summary>
public class MemberVO
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

    /// <summary>
    /// Founder : 创始人
    /// President : 社长,副社长,秘书长
    /// Minister : 部长/副部长
    /// Department : 部员成员
    /// Member : 普通成员
    /// </summary>
    public string Identity { get; set; } = "Member";

    public override string ToString()
    {
        return $"{UserName},{UserId},{Gender},{Academy},{PoliticalLandscape},{ClassName},{PhoneNum},{Identity}";
    }
}
