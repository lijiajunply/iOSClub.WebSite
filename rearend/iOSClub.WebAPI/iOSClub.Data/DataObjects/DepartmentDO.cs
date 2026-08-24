using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iOSClub.Data.DataObjects;

[Table("Departments")]
public class DepartmentDO : DataObject
{
    [MaxLength(32)]
    public string Key { get; set; } = "";

    /// <summary>
    /// 部门名称
    /// </summary>
    [Key]
    [MaxLength(20)]
    public string Name { get; set; } = "";

    [MaxLength(32)]
    public string? Description { get; set; }

    /// <summary>
    /// 部员
    /// </summary>
    public List<StaffDO> Staffs { get; set; } = [];

    public DepartmentDO OutputWhenOtherList()
    {
        Staffs = Staffs.Select(x => x.OutputWhenOtherList()).ToList();
        return this;
    }
}
