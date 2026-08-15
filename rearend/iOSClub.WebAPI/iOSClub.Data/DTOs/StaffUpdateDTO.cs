using System.ComponentModel.DataAnnotations;

namespace iOSClub.Data.DTOs;

public class StaffUpdateDTO
{
    [MaxLength(50)] public string? Name { get; set; }
    [MaxLength(20)] public string? Identity { get; set; }
}
