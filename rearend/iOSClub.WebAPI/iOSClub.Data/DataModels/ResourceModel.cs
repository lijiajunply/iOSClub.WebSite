using System.ComponentModel.DataAnnotations;

namespace iOSClub.Data.DataModels;

public class ResourceModel : DataModel
{
    [Key] [MaxLength(32)] public string Id { get; set; } = "";

    [MaxLength(20)] public string Name { get; set; } = "";

    [MaxLength(512)] public string? Description { get; set; }

    [MaxLength(50)] public string? Tag { get; set; }
}