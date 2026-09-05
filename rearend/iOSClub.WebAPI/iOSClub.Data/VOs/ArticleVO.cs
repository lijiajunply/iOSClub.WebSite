namespace iOSClub.Data.VOs;

/// <summary>
/// 文章视图对象（含完整内容）
/// </summary>
public class ArticleVO
{
    public string Path { get; set; } = "";
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime LastWriteTime { get; set; }
    public string? Identity { get; set; }
    public string? VisibleToDepartment { get; set; }
    public string? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int ArticleOrder { get; set; }
}
