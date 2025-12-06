using iOSClub.Data.DataModels;

namespace iOSClub.DataApi.CQRS.Commands;

// 命令：创建资源
public record CreateResourceCommand(ResourceModel Resource) : ICommand;

// 命令：更新资源
public record UpdateResourceCommand(ResourceModel Resource) : ICommand;

// 命令：删除资源
public record DeleteResourceCommand(string Id) : ICommand;