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

    /// <summary>
    /// 明文密码，由 MapperConfig 映射时用 BCrypt 哈希进 StudentDO.PasswordHash。
    /// 必须在这里就挡住空值：DataTool.StringToHash 对空白输入会抛 ArgumentException，
    /// 而它是在 Mapster 映射表达式里调用的，一旦放行到映射阶段就只能变成 500。
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    [MaxLength(256)]
    public string Password { get; set; } = "";

    [MaxLength(256)] public string? EMail { get; set; }
}
