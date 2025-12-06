using iOSClub.Data.DataModels;

namespace iOSClub.DataApi.CQRS.Commands;

// 命令：创建项目
public record CreateProjectCommand(ProjectModel Project) : ICommand;

// 命令：更新项目
public record UpdateProjectCommand(ProjectModel Project) : ICommand;

// 命令：删除项目
public record DeleteProjectCommand(string Id) : ICommand;

// 命令：批量更新项目
public record UpdateManyProjectsCommand(List<ProjectModel> Projects) : ICommand;