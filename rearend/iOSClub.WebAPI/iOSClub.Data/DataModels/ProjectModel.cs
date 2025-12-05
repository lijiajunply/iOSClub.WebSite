using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace iOSClub.Data.DataModels;

public class ProjectModel : DataModel
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DepartmentModel? Department { get; set; }
    [MaxLength(20)] public string Title { get; set; } = "";

    [Key] [MaxLength(32)]public string Id { get; set; } = "";

    [MaxLength(512)] public string Description { get; set; } = "";
    [MaxLength(20)] public string? StartTime { get; set; }
    [MaxLength(20)] public string? EndTime { get; set; }

    public void Update(ProjectModel model)
    {
        if (!string.IsNullOrEmpty(model.Title)) Title = model.Title;
        if (!string.IsNullOrEmpty(model.Description)) Description = model.Description;
        if (!string.IsNullOrEmpty(model.StartTime)) StartTime = model.StartTime;
        if (!string.IsNullOrEmpty(model.EndTime)) EndTime = model.EndTime;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<StaffModel> Staffs { get; set; } = [];
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<TaskModel> Tasks { get; set; } = [];

    public ProjectModel OutputWhenOtherList()
    {
        Staffs = Staffs.Select(x => x.OutputWhenOtherList()).ToList();
        Tasks = Tasks.Select(x => x.OutputWhenOtherList()).ToList();
        Department = Department?.OutputWhenOtherList();
        return this;
    }
}