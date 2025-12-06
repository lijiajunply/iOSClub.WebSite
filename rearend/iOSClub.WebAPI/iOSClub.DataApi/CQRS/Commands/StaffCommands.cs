using iOSClub.Data.DataModels;

namespace iOSClub.DataApi.CQRS.Commands;

// 命令：创建员工
public record CreateStaffCommand(StaffModel Staff) : ICommand;

// 命令：更新员工
public record UpdateStaffCommand(StaffModel Staff) : ICommand;

// 命令：删除员工
public record DeleteStaffCommand(string UserId) : ICommand;

// 命令：更改员工部门
public record ChangeStaffDepartmentCommand(string UserId, string? DepartmentName) : ICommand;