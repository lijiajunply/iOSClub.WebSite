using System.ComponentModel.DataAnnotations;

namespace iOSClub.Data.DataModels;

public class ArticleModel
{
    /// <summary>
    /// 路径
    /// </summary>
    [Key]
    [MaxLength(128)]
    public string Path { get; set; } = "";

    /// <summary>
    /// 标题
    /// </summary>
    [MaxLength(32)]
    public string Title { get; set; } = "";

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = "";

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastWriteTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Founder : 创始人
    /// President : 社长,团支书
    /// Minister : 部长
    /// Department : 部员成员
    /// </summary>
    [MaxLength(20)]
    public string? Identity { get; set; }

    /// <summary>
    /// 分类ID
    /// </summary>
    [MaxLength(128)]
    public string? CategoryId { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    public CategoryModel? Category { get; set; }

    /// <summary>
    /// 文章排序（用于自定义分类内文章顺序）
    /// </summary>
    public int ArticleOrder { get; set; }
}