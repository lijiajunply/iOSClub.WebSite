using iOSClub.Data.DataModels;
using iOSClub.Data.ShowModels;

namespace iOSClub.DataApi.CQRS.Queries;

// 查询：获取所有员工
public record GetStaffsQuery : IQuery;

// 查询：获取所有员工并转换为成员模型
public record GetStaffsAsMembersQuery : IQuery;

// 查询：根据ID获取员工
public record GetStaffByIdQuery(string UserId) : IQuery;

// 查询：根据ID获取员工，不包含关联数据
public record GetStaffByIdWithoutOtherDataQuery(string UserId) : IQuery;

// 查询：根据身份获取员工
public record GetStaffsByIdentitiesQuery(string[] Identities) : IQuery;