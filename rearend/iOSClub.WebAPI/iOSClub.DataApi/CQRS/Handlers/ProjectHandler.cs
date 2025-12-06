using iOSClub.Data.DataModels;
using iOSClub.DataApi.CQRS.Commands;
using iOSClub.DataApi.CQRS.Queries;
using iOSClub.DataApi.Repositories;

namespace iOSClub.DataApi.CQRS.Handlers;

public class ProjectQueryHandler(IProjectRepository projectRepository) : 
    IQueryHandler<GetProjectsQuery, IEnumerable<ProjectModel>>,
    IQueryHandler<GetProjectByIdQuery, ProjectModel?>
{
    private const string ProjectsCacheKey = "projects:all";
    private const string ProjectCacheKeyPrefix = "projects:";
    private const int CacheExpirationMinutes = 30;

    public async Task<IEnumerable<ProjectModel>> Handle(GetProjectsQuery query, CancellationToken cancellationToken = default)
    {
        // 从仓库获取所有项目
        var projects = await projectRepository.GetAllProjectsAsync();
        return projects;
    }

    public async Task<ProjectModel?> Handle(GetProjectByIdQuery query, CancellationToken cancellationToken = default)
    {
        // 从仓库获取指定ID的项目
        var project = await projectRepository.GetProjectByIdAsync(query.Id);
        return project;
    }
}

public class ProjectCommandHandler(IProjectRepository projectRepository) : 
    ICommandHandler<CreateProjectCommand, bool>,
    ICommandHandler<UpdateProjectCommand, bool>,
    ICommandHandler<DeleteProjectCommand, bool>
{
    public async Task<bool> Handle(CreateProjectCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法创建项目
        var staffModel = new StaffModel { UserId = "system", Name = "System", Identity = "Founder" };
        var result = await projectRepository.CreateProjectAsync(command.Project, staffModel);
        return result != null;
    }

    public async Task<bool> Handle(UpdateProjectCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法更新项目
        var result = await projectRepository.UpdateProjectAsync(command.Project);
        return result;
    }

    public async Task<bool> Handle(DeleteProjectCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法删除项目
        var result = await projectRepository.DeleteProjectAsync(command.Id);
        return result;
    }
}