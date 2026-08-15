namespace iOSClub.Data.VOs;

/// <summary>
/// 分类视图对象
/// </summary>
public class CategoryVO
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Order { get; set; }
    public string? Description { get; set; }
    public List<ArticleListItemVO> Articles { get; set; } = [];
}
