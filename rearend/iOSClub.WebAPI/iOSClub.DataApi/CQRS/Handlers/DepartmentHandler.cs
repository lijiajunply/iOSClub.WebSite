using iOSClub.Data.DataModels;
using iOSClub.DataApi.CQRS.Commands;
using iOSClub.DataApi.CQRS.Queries;
using iOSClub.DataApi.Repositories;
using iOSClub.DataApi.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace iOSClub.DataApi.CQRS.Handlers;

public class DepartmentQueryHandler(IDepartmentRepository departmentRepository, IDistributedCache distributedCache, IDataAccessStatisticsService statisticsService) : 
    IQueryHandler<GetDepartmentsQuery, IEnumerable<DepartmentModel>>,
    IQueryHandler<GetDepartmentByNameQuery, DepartmentModel?>,
    IQueryHandler<GetDepartmentByKeyQuery, DepartmentModel?>
{
    private const string DepartmentsCacheKey = "departments:all";
    private const string DepartmentCacheKeyPrefix = "departments:";
    private const string DepartmentByNameCacheKeyPrefix = "departments:name:";
    private const string DepartmentByKeyCacheKeyPrefix = "departments:key:";
    private const int CacheExpirationMinutes = 30;

    public async Task<IEnumerable<DepartmentModel>> Handle(GetDepartmentsQuery query, CancellationToken cancellationToken = default)
    {
        // 尝试从缓存获取
        var cachedDepartments = await distributedCache.GetStringAsync(DepartmentsCacheKey, cancellationToken);
        IEnumerable<DepartmentModel> departments;
        
        if (!string.IsNullOrEmpty(cachedDepartments))
        {
            departments = JsonConvert.DeserializeObject<IEnumerable<DepartmentModel>>(cachedDepartments)!;
        }
        else
        {
            // 缓存不存在，从数据库获取
            departments = await departmentRepository.GetAllDepartmentsAsync();

            // 存入缓存
            await distributedCache.SetStringAsync(
                DepartmentsCacheKey,
                JsonConvert.SerializeObject(departments),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                cancellationToken);
        }
        
        // 记录访问统计
        await statisticsService.RecordDataAccessAsync("department", "all", "read", cancellationToken);

        return departments;
    }

    public async Task<DepartmentModel?> Handle(GetDepartmentByNameQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{DepartmentByNameCacheKeyPrefix}{query.Name}";
        
        // 尝试从缓存获取
        var cachedDepartment = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        DepartmentModel? department;
        
        if (!string.IsNullOrEmpty(cachedDepartment))
        {
            department = JsonConvert.DeserializeObject<DepartmentModel>(cachedDepartment);
        }
        else
        {
            // 缓存不存在，从数据库获取
            department = await departmentRepository.GetDepartmentByNameAsync(query.Name);

            if (department != null)
            {
                // 存入缓存
                await distributedCache.SetStringAsync(
                    cacheKey,
                    JsonConvert.SerializeObject(department),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                    cancellationToken);
            }
        }
        
        // 记录访问统计
        await statisticsService.RecordDataAccessAsync("department", $"name:{query.Name}", "read", cancellationToken);

        return department;
    }

    public async Task<DepartmentModel?> Handle(GetDepartmentByKeyQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{DepartmentByKeyCacheKeyPrefix}{query.Key}";
        
        // 尝试从缓存获取
        var cachedDepartment = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        DepartmentModel? department;
        
        if (!string.IsNullOrEmpty(cachedDepartment))
        {
            department = JsonConvert.DeserializeObject<DepartmentModel>(cachedDepartment);
        }
        else
        {
            // 缓存不存在，从数据库获取
            department = await departmentRepository.GetDepartmentByKeyAsync(query.Key);

            if (department != null)
            {
                // 存入缓存
                await distributedCache.SetStringAsync(
                    cacheKey,
                    JsonConvert.SerializeObject(department),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                    cancellationToken);
            }
        }
        
        // 记录访问统计
        await statisticsService.RecordDataAccessAsync("department", $"key:{query.Key}", "read", cancellationToken);

        return department;
    }
}

public class DepartmentCommandHandler(IDepartmentRepository departmentRepository, IDistributedCache distributedCache, IDataAccessStatisticsService statisticsService) : 
    ICommandHandler<CreateDepartmentCommand, bool>,
    ICommandHandler<UpdateDepartmentCommand, bool>,
    ICommandHandler<DeleteDepartmentCommand, bool>
{
    private const string DepartmentsCacheKey = "departments:all";
    private const string DepartmentCacheKeyPrefix = "departments:";
    private const string DepartmentByNameCacheKeyPrefix = "departments:name:";
    private const string DepartmentByKeyCacheKeyPrefix = "departments:key:";

    public async Task<bool> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法创建部门
        var result = await departmentRepository.AddDepartmentAsync(command.Department);
        
        if (result)
        {
            // 清除相关缓存
            await ClearDepartmentCache(command.Department, cancellationToken);
            
            // 记录变化统计
            await statisticsService.RecordDataAccessAsync("department", command.Department.Name, "create", cancellationToken);
        }
        
        return result;
    }

    public async Task<bool> Handle(UpdateDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法更新部门
        var result = await departmentRepository.UpdateDepartmentAsync(command.Department);
        
        if (result)
        {
            // 清除相关缓存
            await ClearDepartmentCache(command.Department, cancellationToken);
            
            // 记录变化统计
            await statisticsService.RecordDataAccessAsync("department", command.Department.Name, "update", cancellationToken);
        }
        
        return result;
    }

    public async Task<bool> Handle(DeleteDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法删除部门
        var result = await departmentRepository.DeleteDepartmentAsync(command.Name);
        
        if (result)
        {
            // 清除相关缓存
            await ClearDepartmentCacheByName(command.Name, cancellationToken);
            
            // 记录变化统计
            await statisticsService.RecordDataAccessAsync("department", command.Name, "delete", cancellationToken);
        }
        
        return result;
    }
    
    // 清除部门相关缓存的辅助方法
    private async Task ClearDepartmentCache(DepartmentModel department, CancellationToken cancellationToken)
    {
        // 清除所有部门缓存
        await distributedCache.RemoveAsync(DepartmentsCacheKey, cancellationToken);
        
        // 清除单个部门缓存
        await distributedCache.RemoveAsync($"{DepartmentCacheKeyPrefix}{department.Name}", cancellationToken);
        
        // 清除按名称缓存的部门
        await distributedCache.RemoveAsync($"{DepartmentByNameCacheKeyPrefix}{department.Name}", cancellationToken);
        
        // 清除按键缓存的部门
        if (!string.IsNullOrEmpty(department.Key))
        {
            await distributedCache.RemoveAsync($"{DepartmentByKeyCacheKeyPrefix}{department.Key}", cancellationToken);
        }
    }
    
    // 根据名称清除部门相关缓存的辅助方法
    private async Task ClearDepartmentCacheByName(string departmentName, CancellationToken cancellationToken)
    {
        // 清除所有部门缓存
        await distributedCache.RemoveAsync(DepartmentsCacheKey, cancellationToken);
        
        // 清除单个部门缓存
        await distributedCache.RemoveAsync($"{DepartmentCacheKeyPrefix}{departmentName}", cancellationToken);
        
        // 清除按名称缓存的部门
        await distributedCache.RemoveAsync($"{DepartmentByNameCacheKeyPrefix}{departmentName}", cancellationToken);
    }
}