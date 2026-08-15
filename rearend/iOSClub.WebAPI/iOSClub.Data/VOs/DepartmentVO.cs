namespace iOSClub.Data.VOs;

/// <summary>
/// 部门视图对象
/// </summary>
public class DepartmentVO
{
    public string Name { get; set; } = "";
    public string Key { get; set; } = "";
    public string? Description { get; set; }
    public int StaffCount { get; set; }
    public int ProjectCount { get; set; }
}
