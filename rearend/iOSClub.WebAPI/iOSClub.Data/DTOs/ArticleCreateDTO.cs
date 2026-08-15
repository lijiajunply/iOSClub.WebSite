using System.ComponentModel.DataAnnotations;

namespace iOSClub.Data.DTOs;

public class ArticleCreateDTO
{
    [Required(ErrorMessage = "文章路径是必需的")]
    [StringLength(128, ErrorMessage = "路径长度不能超过128个字符")]
    [RegularExpression("^[a-zA-Z0-9_-]+$", ErrorMessage = "路径只能包含字母、数字、下划线和连字符")]
    public string Path { get; set; } = "";

    [Required(ErrorMessage = "文章标题是必需的")]
    [StringLength(100, ErrorMessage = "标题长度不能超过100个字符")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "文章内容是必需的")]
    [MinLength(10, ErrorMessage = "内容至少需要10个字符")]
    public string Content { get; set; } = "";

    [StringLength(20, ErrorMessage = "身份标识长度不能超过20个字符")]
    public string? Identity { get; set; }

    [StringLength(128, ErrorMessage = "分类长度不能超过128个字符")]
    public string? Category { get; set; }

    [Range(0, 1000, ErrorMessage = "文章排序值必须在0-1000之间")]
    public int ArticleOrder { get; set; } = 0;
}
