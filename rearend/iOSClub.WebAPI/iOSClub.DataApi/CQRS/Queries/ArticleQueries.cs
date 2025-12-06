using iOSClub.Data.DataModels;

namespace iOSClub.DataApi.CQRS.Queries;

// 查询：获取所有文章
public record GetArticlesQuery : IQuery;

// 查询：根据ID获取文章
public record GetArticleByIdQuery(string Id) : IQuery;

// 查询：获取分页文章列表
public record GetArticlesPagedQuery(int PageNum, int PageSize, string? SearchTerm = null, string? SearchCondition = null) : IQuery;

// 查询：根据分类获取文章
public record GetArticlesByCategoryQuery(string CategoryId) : IQuery;