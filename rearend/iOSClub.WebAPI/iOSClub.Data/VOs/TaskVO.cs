namespace iOSClub.Data.VOs;

/// <summary>
/// 任务视图对象
/// </summary>
public class TaskVO
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string StartTime { get; set; } = "";
    public string EndTime { get; set; } = "";
    public bool Status { get; set; }
}
