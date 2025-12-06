using iOSClub.Data.DataModels;

namespace iOSClub.DataApi.CQRS.Queries;

// 查询：获取所有分类
public record GetCategoriesQuery : IQuery;

// 查询：根据ID获取分类
public record GetCategoryByIdQuery(string Id) : IQuery;

// 查询：根据名称获取分类
public record GetCategoryByNameQuery(string Name) : IQuery;