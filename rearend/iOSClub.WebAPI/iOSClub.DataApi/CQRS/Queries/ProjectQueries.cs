using iOSClub.Data.DataModels;

namespace iOSClub.DataApi.CQRS.Queries;

// 查询：获取所有项目
public record GetProjectsQuery : IQuery;

// 查询：根据ID获取项目
public record GetProjectByIdQuery(string Id) : IQuery;

// 查询：获取分页项目列表
public record GetProjectsPagedQuery(int PageNum, int PageSize, string? SearchTerm = null, string? SearchCondition = null) : IQuery;