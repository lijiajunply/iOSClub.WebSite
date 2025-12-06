namespace iOSClub.DataApi.CQRS;

// 命令标记接口，用于表示修改数据的操作
public interface ICommand { }

// 命令处理程序接口，定义命令处理方法
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand
{
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken = default);
}
