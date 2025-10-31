using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace iOSClub.Data.DataModels;

public class StaffModel
{
    [Key]
    [MaxLength(10)]
    public string UserId { get; set; } = "";

    [MaxLength(50)] public string Name { get; set; } = "";

    /// <summary>
    /// Founder : 创始人
    /// President : 社长,团支书,秘书长
    /// Minister : 部长
    /// Department : 部员成员
    /// Member : 普通成员
    /// </summary>
    [MaxLength(20)]
    public string Identity { get; set; } = "Member";

    [JsonIgnore]
    public DepartmentModel? Department { get; set; }
    
    public List<ProjectModel> Projects { get; init; } = [];
    public List<TaskModel> Tasks { get; init; } = [];
}