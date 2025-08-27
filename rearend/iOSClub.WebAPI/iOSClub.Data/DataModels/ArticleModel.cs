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
    public DateTime LastWriteTime { get; set; } = DateTime.Now;

    /// <summary>
    /// Founder : 创始人
    /// President : 社长,团支书
    /// Minister : 部长
    /// Department : 部员成员
    /// </summary>
    [MaxLength(20)]
    public string? Identity { get; set; }
}