namespace iOSClub.DataApi.CQRS;

// 查询标记接口，用于表示只读数据操作
public interface IQuery { }

// 查询处理程序接口，定义查询处理方法
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery
{
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken = default);
}
