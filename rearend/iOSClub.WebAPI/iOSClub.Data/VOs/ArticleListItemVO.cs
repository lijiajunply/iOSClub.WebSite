namespace iOSClub.Data.VOs;

/// <summary>
/// 文章列表视图对象（不含完整内容，节省带宽）
/// </summary>
public class ArticleListItemVO
{
    public string Path { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime LastWriteTime { get; set; }
    public string? Identity { get; set; }
    public string? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int ArticleOrder { get; set; }
}
