using System.ComponentModel.DataAnnotations;

namespace iOSClub.Data.DataModels;

public class CategoryModel : DataModel
{
    /// <summary>
    /// 分类ID
    /// </summary>
    [Key]
    [MaxLength(128)]
    public string Id { get; set; } = "";

    /// <summary>
    /// 分类名称
    /// </summary>
    [Required(ErrorMessage = "分类名称是必需的")]
    [MaxLength(128, ErrorMessage = "分类名称长度不能超过128个字符")]
    public string Name { get; set; } = "";

    /// <summary>
    /// 分类排序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 分类描述
    /// </summary>
    [MaxLength(512, ErrorMessage = "分类描述长度不能超过512个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 分类下的文章
    /// </summary>
    public ICollection<ArticleModel> Articles { get; set; } = new List<ArticleModel>();
}