using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iOSClub.Data.DataObjects;

[Table("Todos")]
public class TodoDO : DataObject, ITodo
{
    [MaxLength(20)] public string Title { get; set; } = "";
    [MaxLength(200)] public string Description { get; set; } = "";
    [MaxLength(20)] public string StartTime { get; set; } = "";
    [MaxLength(20)] public string EndTime { get; set; } = "";

    public bool Status { get; set; }

    [Key]
    [MaxLength(32)]
    public string Id { get; set; } = "";

    public StudentDO Student { get; set; } = new();

    [MaxLength(10)] public string StudentId { get; set; } = "";
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    public void Update(ITodo model)
    {
        if (!string.IsNullOrEmpty(model.Title)) Title = model.Title;
        if (!string.IsNullOrEmpty(model.Description)) Description = model.Description;
        if (!string.IsNullOrEmpty(model.StartTime)) StartTime = model.StartTime;
        if (!string.IsNullOrEmpty(model.EndTime)) EndTime = model.EndTime;
        Status = model.Status;
    }
}
