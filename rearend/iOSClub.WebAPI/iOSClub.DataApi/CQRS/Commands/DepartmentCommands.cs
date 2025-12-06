using iOSClub.Data.DataModels;

namespace iOSClub.DataApi.CQRS.Commands;

// 命令：创建部门
public record CreateDepartmentCommand(DepartmentModel Department) : ICommand;

// 命令：更新部门
public record UpdateDepartmentCommand(DepartmentModel Department) : ICommand;

// 命令：删除部门
public record DeleteDepartmentCommand(string Name) : ICommand;