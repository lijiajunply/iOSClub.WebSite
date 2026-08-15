using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace iOSClub.Data.DataObjects;

[Table("Projects")]
public class ProjectDO : DataObject
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DepartmentDO? Department { get; set; }
    [MaxLength(20)] public string Title { get; set; } = "";

    [Key] [MaxLength(32)]public string Id { get; set; } = "";

    [MaxLength(512)] public string Description { get; set; } = "";
    [MaxLength(20)] public string? StartTime { get; set; }
    [MaxLength(20)] public string? EndTime { get; set; }

    public void Update(ProjectDO model)
    {
        if (!string.IsNullOrEmpty(model.Title)) Title = model.Title;
        if (!string.IsNullOrEmpty(model.Description)) Description = model.Description;
        if (!string.IsNullOrEmpty(model.StartTime)) StartTime = model.StartTime;
        if (!string.IsNullOrEmpty(model.EndTime)) EndTime = model.EndTime;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<StaffDO> Staffs { get; set; } = [];

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<TaskDO> Tasks { get; set; } = [];

    public ProjectDO OutputWhenOtherList()
    {
        Staffs = Staffs.Select(x => x.OutputWhenOtherList()).ToList();
        Tasks = Tasks.Select(x => x.OutputWhenOtherList()).ToList();
        Department = Department?.OutputWhenOtherList();
        return this;
    }
}
