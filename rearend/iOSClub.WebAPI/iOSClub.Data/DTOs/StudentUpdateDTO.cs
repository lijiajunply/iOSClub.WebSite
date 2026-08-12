using System.ComponentModel.DataAnnotations;

namespace iOSClub.Data.DTOs;

public class StudentUpdateDTO
{
    [MaxLength(50)] public string? UserName { get; set; }
    [MaxLength(50)] public string? Academy { get; set; }
    [MaxLength(10)] public string? PoliticalLandscape { get; set; }
    [MaxLength(2)] public string? Gender { get; set; }
    [MaxLength(20)] public string? ClassName { get; set; }
    [MaxLength(14)] public string? PhoneNum { get; set; }
    [MaxLength(256)] public string? EMail { get; set; }
}
