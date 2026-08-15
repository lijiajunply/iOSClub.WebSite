using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace iOSClub.Data.DataObjects;

[Table("Tasks")]
public class TaskDO : DataObject, ITodo
{
    [JsonIgnore] public ProjectDO Project { get; set; } = new();
    [MaxLength(20)] public string Title { get; set; } = "";
    [MaxLength(200)] public string Description { get; set; } = "";
    [MaxLength(20)] public string StartTime { get; set; } = "";
    [MaxLength(20)] public string EndTime { get; set; } = "";

    public bool Status { get; set; }

    [Key] [MaxLength(32)] public string Id { get; set; } = "";

    public void Update(ITodo model)
    {
        if (!string.IsNullOrEmpty(model.Title)) Title = model.Title;
        if (!string.IsNullOrEmpty(model.Description)) Description = model.Description;
        if (!string.IsNullOrEmpty(model.StartTime)) StartTime = model.StartTime;
        if (!string.IsNullOrEmpty(model.EndTime)) EndTime = model.EndTime;
        Status = model.Status;
    }

    [JsonIgnore] public List<StaffDO> Users { get; set; } = [];

    public TaskDO OutputWhenOtherList()
    {
        Users = Users.Select(x => x.OutputWhenOtherList()).ToList();
        Project = Project.OutputWhenOtherList();
        return this;
    }
}
