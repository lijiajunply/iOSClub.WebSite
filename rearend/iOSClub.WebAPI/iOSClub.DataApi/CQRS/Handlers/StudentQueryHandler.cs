using iOSClub.Data.DataModels;
using iOSClub.Data.ShowModels;
using iOSClub.DataApi.CQRS.Queries;
using iOSClub.DataApi.Repositories;
using iOSClub.DataApi.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace iOSClub.DataApi.CQRS.Handlers;

public class StudentQueryHandler(IStudentRepository studentRepository, IDistributedCache distributedCache, IDataAccessStatisticsService statisticsService) :
    IQueryHandler<GetStudentsQuery, List<StudentModel>>,
    IQueryHandler<GetStudentByIdQuery, StudentModel?>,
    IQueryHandler<GetStudentsPagedQuery, (List<MemberModel>, int)>
{
    private const string StudentsCacheKey = "students:all";
    private const string StudentCacheKeyPrefix = "students:";
    private const string StudentsPagedCacheKeyPrefix = "students:paged:";
    private const int CacheExpirationMinutes = 30;
    private const int PagedCacheExpirationMinutes = 5;

    public async Task<List<StudentModel>> Handle(GetStudentsQuery query, CancellationToken cancellationToken = default)
    {
        // 尝试从缓存获取
        var cachedStudents = await distributedCache.GetStringAsync(StudentsCacheKey, cancellationToken);
        List<StudentModel> students;
        
        if (!string.IsNullOrEmpty(cachedStudents))
        {
            students = JsonConvert.DeserializeObject<List<StudentModel>>(cachedStudents)!;
        }
        else
        {
            // 缓存不存在，从数据库获取
            students = await studentRepository.GetAll();

            // 存入缓存
            await distributedCache.SetStringAsync(
                StudentsCacheKey,
                JsonConvert.SerializeObject(students),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                cancellationToken);
        }
        
        // 记录访问统计
        await statisticsService.RecordDataAccessAsync("student", "all", "read", cancellationToken);

        return students;
    }

    public async Task<StudentModel?> Handle(GetStudentByIdQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{StudentCacheKeyPrefix}{query.Id}";
        
        // 尝试从缓存获取
        var cachedStudent = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        StudentModel? student;
        
        if (!string.IsNullOrEmpty(cachedStudent))
        {
            student = JsonConvert.DeserializeObject<StudentModel>(cachedStudent);
        }
        else
        {
            // 缓存不存在，从数据库获取
            student = await studentRepository.GetByIdAsync(query.Id);

            if (student != null)
            {
                // 存入缓存
                await distributedCache.SetStringAsync(
                    cacheKey,
                    JsonConvert.SerializeObject(student),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                    cancellationToken);
            }
        }
        
        // 记录访问统计
        await statisticsService.RecordDataAccessAsync("student", query.Id, "read", cancellationToken);

        return student;
    }

    public async Task<(List<MemberModel>, int)> Handle(GetStudentsPagedQuery query, CancellationToken cancellationToken = default)
    {
        // 分页查询使用短期缓存，因为数据会频繁变化
        var cacheKey = $"{StudentsPagedCacheKeyPrefix}{query.PageNum}_{query.PageSize}_{query.SearchTerm ?? ""}_{query.SearchCondition ?? ""}";
        
        // 尝试从缓存获取
        var cachedResult = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        (List<MemberModel>, int) result;
        
        if (!string.IsNullOrEmpty(cachedResult))
        {
            result = JsonConvert.DeserializeObject<(List<MemberModel>, int)>(cachedResult)!;
        }
        else
        {
            // 缓存不存在，从数据库获取
            result = await studentRepository.GetMembersPagedAsync(query.PageNum, query.PageSize, query.SearchTerm, query.SearchCondition);

            // 存入缓存，设置较短的过期时间
            await distributedCache.SetStringAsync(
                cacheKey,
                JsonConvert.SerializeObject(result),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PagedCacheExpirationMinutes) },
                cancellationToken);
        }
        
        // 记录访问统计
        await statisticsService.RecordDataAccessAsync("student", "paged", "read", cancellationToken);

        return result;
    }
}
