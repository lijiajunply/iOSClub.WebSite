using iOSClub.Data.DataModels;

namespace iOSClub.DataApi.CQRS.Commands;

// 命令：创建学生
public record CreateStudentCommand(StudentModel Student) : ICommand;

// 命令：更新学生
public record UpdateStudentCommand(StudentModel Student) : ICommand;

// 命令：删除学生
public record DeleteStudentCommand(string Id) : ICommand;

// 命令：批量更新学生
public record UpdateManyStudentsCommand(List<StudentModel> Students) : ICommand;

// 命令：学生登录
public record StudentLoginCommand(string UserId, string Password) : ICommand;
