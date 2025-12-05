using iOSClub.Data.DataModels;
using iOSClub.Data.ShowModels;
using iOSClub.DataApi.CQRS.Queries;
using iOSClub.DataApi.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace iOSClub.DataApi.CQRS.Handlers;

public class StudentQueryHandler(IStudentRepository studentRepository, IDistributedCache distributedCache) :
    IQueryHandler<GetStudentsQuery, List<StudentModel>>,
    IQueryHandler<GetStudentByIdQuery, StudentModel?>,
    IQueryHandler<GetStudentsPagedQuery, (List<MemberModel>, int)>
{
    private const string StudentsCacheKey = "students:all";
    private const string StudentCacheKeyPrefix = "students:";
    private const string StudentsPagedCacheKeyPrefix = "students:paged:";
    private const int CacheExpirationMinutes = 30;

    public async Task<List<StudentModel>> Handle(GetStudentsQuery query, CancellationToken cancellationToken = default)
    {
        // 尝试从缓存获取
        var cachedStudents = await distributedCache.GetStringAsync(StudentsCacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedStudents))
        {
            return JsonConvert.DeserializeObject<List<StudentModel>>(cachedStudents)!;
        }

        // 缓存不存在，从数据库获取
        var students = await studentRepository.GetAll();

        // 存入缓存
        await distributedCache.SetStringAsync(
            StudentsCacheKey,
            JsonConvert.SerializeObject(students),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
            cancellationToken);

        return students;
    }

    public async Task<StudentModel?> Handle(GetStudentByIdQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{StudentCacheKeyPrefix}{query.Id}";
        
        // 尝试从缓存获取
        var cachedStudent = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedStudent))
        {
            return JsonConvert.DeserializeObject<StudentModel>(cachedStudent);
        }

        // 缓存不存在，从数据库获取
        var student = await studentRepository.GetByIdAsync(query.Id);

        if (student != null)
        {
            // 存入缓存
            await distributedCache.SetStringAsync(
                cacheKey,
                JsonConvert.SerializeObject(student),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                cancellationToken);
        }

        return student;
    }

    public async Task<(List<MemberModel>, int)> Handle(GetStudentsPagedQuery query, CancellationToken cancellationToken = default)
    {
        // 分页查询不缓存，因为数据会频繁变化
        return await studentRepository.GetMembersPagedAsync(query.PageNum, query.PageSize, query.SearchTerm, query.SearchCondition);
    }
}
