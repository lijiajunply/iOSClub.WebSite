using iOSClub.Data.DataModels;

namespace iOSClub.DataApi.CQRS.Queries;

// 查询：获取所有学生
public record GetStudentsQuery : IQuery;

// 查询：根据ID获取学生
public record GetStudentByIdQuery(string Id) : IQuery;

// 查询：获取分页学生列表
public record GetStudentsPagedQuery(int PageNum, int PageSize, string? SearchTerm = null, string? SearchCondition = null) : IQuery;
