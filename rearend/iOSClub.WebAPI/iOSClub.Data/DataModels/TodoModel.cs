using System.ComponentModel.DataAnnotations;

namespace iOSClub.Data.DataModels;

public class TodoModel : DataModel, ITodo
{
    [MaxLength(20)] public string Title { get; set; } = "";
    [MaxLength(200)] public string Description { get; set; } = "";
    [MaxLength(20)] public string StartTime { get; set; } = "";
    [MaxLength(20)] public string EndTime { get; set; } = "";

    public bool Status { get; set; }

    [Key]
    [MaxLength(32)]
    public string Id { get; set; } = "";

    public StudentModel Student { get; set; } = new();

    [MaxLength(10)] public string StudentId { get; set; } = "";
    
    public void Update(ITodo model)
    {
        if (!string.IsNullOrEmpty(model.Title)) Title = model.Title;
        if (!string.IsNullOrEmpty(model.Description)) Description = model.Description;
        if (!string.IsNullOrEmpty(model.StartTime)) StartTime = model.StartTime;
        if (!string.IsNullOrEmpty(model.EndTime)) EndTime = model.EndTime;
        Status = model.Status;
    }
}