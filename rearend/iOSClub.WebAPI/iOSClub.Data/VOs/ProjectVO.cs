namespace iOSClub.Data.VOs;

/// <summary>
/// 项目视图对象
/// </summary>
public class ProjectVO
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? DepartmentName { get; set; }
    public List<ProjectStaffSummary> Staffs { get; set; } = [];
    public List<ProjectTaskSummary> Tasks { get; set; } = [];
}

public class ProjectStaffSummary
{
    public string UserId { get; set; } = "";
    public string Name { get; set; } = "";
}

public class ProjectTaskSummary
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public bool Status { get; set; }
}
