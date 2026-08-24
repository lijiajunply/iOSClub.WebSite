namespace iOSClub.Data.VOs;

/// <summary>
/// 员工视图对象 - Department展平为DepartmentName
/// </summary>
public class StaffVO
{
    public string UserId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Identity { get; set; } = "Member";
    public string? DepartmentName { get; set; }
}
