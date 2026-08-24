using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iOSClub.Data.DataObjects;

[Table("Staffs")]
public class StaffDO
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

    public DepartmentDO? Department { get; set; }

    public StaffDO OutputWhenOtherList()
    {
        Department = Department == null
            ? null
            : new DepartmentDO()
            {
                Key = Department.Key,
                Name = Department.Name,
                Description = Department.Description,
            };

        return this;
    }
}
