using iOSClub.Data.DataModels;
using iOSClub.DataApi.CQRS.Commands;
using iOSClub.DataApi.CQRS.Queries;
using iOSClub.DataApi.Repositories;
using iOSClub.DataApi.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace iOSClub.DataApi.CQRS.Handlers;

public class ProjectQueryHandler(IProjectRepository projectRepository, IDistributedCache distributedCache, IDataAccessStatisticsService statisticsService) : 
    IQueryHandler<GetProjectsQuery, IEnumerable<ProjectModel>>,
    IQueryHandler<GetProjectByIdQuery, ProjectModel?>
{
    private const string ProjectsCacheKey = "projects:all";
    private const string ProjectCacheKeyPrefix = "projects:";
    private const int CacheExpirationMinutes = 30;

    public async Task<IEnumerable<ProjectModel>> Handle(GetProjectsQuery query, CancellationToken cancellationToken = default)
    {
        // 尝试从缓存获取
        var cachedProjects = await distributedCache.GetStringAsync(ProjectsCacheKey, cancellationToken);
        IEnumerable<ProjectModel> projects;
        
        if (!string.IsNullOrEmpty(cachedProjects))
        {
            projects = JsonConvert.DeserializeObject<IEnumerable<ProjectModel>>(cachedProjects)!;
        }
        else
        {
            // 缓存不存在，从数据库获取
            projects = await projectRepository.GetAllProjectsAsync();

            // 存入缓存
            await distributedCache.SetStringAsync(
                ProjectsCacheKey,
                JsonConvert.SerializeObject(projects),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                cancellationToken);
        }
        
        // 记录访问统计
        await statisticsService.RecordDataAccessAsync("project", "all", "read", cancellationToken);

        return projects;
    }

    public async Task<ProjectModel?> Handle(GetProjectByIdQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ProjectCacheKeyPrefix}{query.Id}";
        
        // 尝试从缓存获取
        var cachedProject = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        ProjectModel? project;
        
        if (!string.IsNullOrEmpty(cachedProject))
        {
            project = JsonConvert.DeserializeObject<ProjectModel>(cachedProject);
        }
        else
        {
            // 缓存不存在，从数据库获取
            project = await projectRepository.GetProjectByIdAsync(query.Id);

            if (project != null)
            {
                // 存入缓存
                await distributedCache.SetStringAsync(
                    cacheKey,
                    JsonConvert.SerializeObject(project),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                    cancellationToken);
            }
        }
        
        // 记录访问统计
        await statisticsService.RecordDataAccessAsync("project", query.Id, "read", cancellationToken);

        return project;
    }
}

public class ProjectCommandHandler(IProjectRepository projectRepository, IDistributedCache distributedCache, IDataAccessStatisticsService statisticsService) : 
    ICommandHandler<CreateProjectCommand, bool>,
    ICommandHandler<UpdateProjectCommand, bool>,
    ICommandHandler<DeleteProjectCommand, bool>
{
    private const string ProjectsCacheKey = "projects:all";
    private const string ProjectCacheKeyPrefix = "projects:";

    public async Task<bool> Handle(CreateProjectCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法创建项目
        var staffModel = new StaffModel { UserId = "system", Name = "System", Identity = "Founder" };
        var result = await projectRepository.CreateProjectAsync(command.Project, staffModel);
        
        if (result != null)
        {
            // 清除相关缓存
            await ClearProjectCache(result.Id, cancellationToken);
            
            // 记录变化统计
            await statisticsService.RecordDataAccessAsync("project", result.Id, "create", cancellationToken);
            return true;
        }
        
        return false;
    }

    public async Task<bool> Handle(UpdateProjectCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法更新项目
        var result = await projectRepository.UpdateProjectAsync(command.Project);
        
        if (result)
        {
            // 清除相关缓存
            await ClearProjectCache(command.Project.Id, cancellationToken);
            
            // 记录变化统计
            await statisticsService.RecordDataAccessAsync("project", command.Project.Id, "update", cancellationToken);
        }
        
        return result;
    }

    public async Task<bool> Handle(DeleteProjectCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法删除项目
        var result = await projectRepository.DeleteProjectAsync(command.Id);
        
        if (result)
        {
            // 清除相关缓存
            await ClearProjectCache(command.Id, cancellationToken);
            
            // 记录变化统计
            await statisticsService.RecordDataAccessAsync("project", command.Id, "delete", cancellationToken);
        }
        
        return result;
    }
    
    // 清除项目相关缓存的辅助方法
    private async Task ClearProjectCache(string projectId, CancellationToken cancellationToken)
    {
        // 清除所有项目缓存
        await distributedCache.RemoveAsync(ProjectsCacheKey, cancellationToken);
        
        // 清除单个项目缓存
        await distributedCache.RemoveAsync($"{ProjectCacheKeyPrefix}{projectId}", cancellationToken);
    }
}