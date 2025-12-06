using iOSClub.Data.DataModels;

namespace iOSClub.DataApi.CQRS.Queries;

// 查询：获取所有资源
public record GetResourcesQuery : IQuery;

// 查询：根据ID获取资源
public record GetResourceByIdQuery(string Id) : IQuery;

// 查询：根据标签获取资源
public record GetResourcesByTagQuery(string Tag) : IQuery;

// 查询：根据名称搜索资源
public record SearchResourcesByNameQuery(string Name) : IQuery;