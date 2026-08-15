using System.ComponentModel.DataAnnotations;

namespace iOSClub.Data.DTOs;

public class TodoCreateUpdateDTO
{
    [MaxLength(32)] public string? Id { get; set; }
    [MaxLength(20)] public string Title { get; set; } = "";
    [MaxLength(200)] public string Description { get; set; } = "";
    [MaxLength(20)] public string StartTime { get; set; } = "";
    [MaxLength(20)] public string EndTime { get; set; } = "";
    public bool Status { get; set; }
}
