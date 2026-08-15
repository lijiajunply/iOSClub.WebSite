using System.ComponentModel.DataAnnotations;

namespace iOSClub.Data.DTOs;

public class StudentCreateDTO
{
    [Required(ErrorMessage = "用户ID是必需的")]
    [MaxLength(10, ErrorMessage = "用户ID长度不能超过10个字符")]
    public string UserId { get; set; } = "";

    [Required(ErrorMessage = "用户名是必需的")]
    [MaxLength(50, ErrorMessage = "用户名长度不能超过50个字符")]
    public string UserName { get; set; } = "";

    [MaxLength(50)] public string Academy { get; set; } = "";
    [MaxLength(10)] public string PoliticalLandscape { get; set; } = "群众";
    [MaxLength(2)] public string Gender { get; set; } = "";
    [MaxLength(20)] public string ClassName { get; set; } = "";
    [MaxLength(14)] public string PhoneNum { get; set; } = "";
    [MaxLength(256)] public string Password { get; set; } = "";
    [MaxLength(256)] public string? EMail { get; set; }
}
