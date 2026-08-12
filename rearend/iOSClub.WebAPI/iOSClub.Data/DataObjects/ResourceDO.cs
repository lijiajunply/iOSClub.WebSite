using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iOSClub.Data.DataObjects;

[Table("Resources")]
public class ResourceDO : DataObject
{
    [Key] [MaxLength(32)] public string Id { get; set; } = "";

    [MaxLength(20)] public string Name { get; set; } = "";

    [MaxLength(512)] public string? Description { get; set; }

    [MaxLength(50)] public string? Tag { get; set; }
}
