using System.ComponentModel.DataAnnotations;

namespace iOSClub.Data.DTOs;

public class DepartmentCreateUpdateDTO
{
    [Required][MaxLength(20)] public string Name { get; set; } = "";
    [MaxLength(32)] public string Key { get; set; } = "";
    [MaxLength(32)] public string? Description { get; set; }
}
