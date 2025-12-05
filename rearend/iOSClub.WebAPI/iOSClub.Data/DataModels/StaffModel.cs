using System.ComponentModel.DataAnnotations;

namespace iOSClub.Data.DataModels;

public class StaffModel
{
    [Key] [MaxLength(10)] public string UserId { get; set; } = "";

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

    public DepartmentModel? Department { get; set; }

    public List<ProjectModel> Projects { get; set; } = [];
    public List<TaskModel> Tasks { get; set; } = [];

    public StaffModel OutputWhenOtherList()
    {
        Department = Department == null
            ? null
            : new DepartmentModel()
            {
                Key = Department.Key,
                Name = Department.Name,
                Description = Department.Description,
            };

        Tasks = Tasks.Select(t => t.OutputWhenOtherList()).ToList();
        Projects = Projects.Select(p => p.OutputWhenOtherList()).ToList();

        return this;
    }
}