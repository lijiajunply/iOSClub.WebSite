using iOSClub.Data.DataModels;

namespace iOSClub.DataApi.CQRS.Queries;

// 查询：获取所有部门
public record GetDepartmentsQuery : IQuery;

// 查询：根据名称获取部门
public record GetDepartmentByNameQuery(string Name) : IQuery;

// 查询：根据Key获取部门
public record GetDepartmentByKeyQuery(string Key) : IQuery;