using iOSClub.Data.DataObjects;

namespace iOSClub.Data.DTOs;

[Serializable]
public class AllDataImportDTO
{
    public List<StudentDO> Students { get; init; } = [];
    public List<StaffDO> Presidents { get; init; } = [];
    public List<ResourceDO> Resources { get; init; } = [];
    public List<DepartmentDO> Departments { get; init; } = [];
    public List<ArticleDO> Articles { get; init; } = [];
}
