using System.ComponentModel.DataAnnotations;

namespace iOSClub.Data.DTOs;

public class CategoryCreateUpdateDTO
{
    [Required(ErrorMessage = "分类名称是必需的")]
    [MaxLength(128, ErrorMessage = "分类名称长度不能超过128个字符")]
    public string Name { get; set; } = "";

    [MaxLength(512, ErrorMessage = "分类描述长度不能超过512个字符")]
    public string? Description { get; set; }

    public int Order { get; set; }
}
