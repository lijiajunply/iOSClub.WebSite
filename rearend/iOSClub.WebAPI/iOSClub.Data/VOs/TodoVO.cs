namespace iOSClub.Data.VOs;

/// <summary>
/// 待办事项视图对象
/// </summary>
public class TodoVO
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string StartTime { get; set; } = "";
    public string EndTime { get; set; } = "";
    public bool Status { get; set; }
    public string StudentId { get; set; } = "";
    public DateTime CreatedTime { get; set; }
}
