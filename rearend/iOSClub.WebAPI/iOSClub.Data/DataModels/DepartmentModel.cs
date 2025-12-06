using System.ComponentModel.DataAnnotations;

namespace iOSClub.Data.DataModels;

public class DepartmentModel : DataModel
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
    public List<StaffModel> Staffs { get; set; } = [];

    /// <summary>
    /// 项目
    /// </summary>
    public List<ProjectModel> Projects { get; set; } = [];

    public DepartmentModel OutputWhenOtherList()
    {
        Staffs = Staffs.Select(x => x.OutputWhenOtherList()).ToList();
        Projects = Projects.Select(x => x.OutputWhenOtherList()).ToList();
        return this;
    }
}