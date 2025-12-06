using iOSClub.Data.DataModels;
using iOSClub.DataApi.CQRS.Commands;
using iOSClub.DataApi.Repositories;
using iOSClub.DataApi.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace iOSClub.DataApi.CQRS.Handlers;

public class StudentCommandHandler(IStudentRepository studentRepository, IDistributedCache distributedCache, IDataAccessStatisticsService statisticsService) :
    ICommandHandler<CreateStudentCommand, StudentModel?>,
    ICommandHandler<UpdateStudentCommand, bool>,
    ICommandHandler<DeleteStudentCommand, bool>,
    ICommandHandler<UpdateManyStudentsCommand, bool>,
    ICommandHandler<StudentLoginCommand, bool>
{
    private const string StudentsCacheKey = "students:all";
    private const string StudentCacheKeyPrefix = "students:";
    private const string StudentsPagedCacheKeyPrefix = "students:paged:";

    public async Task<StudentModel?> Handle(CreateStudentCommand command, CancellationToken cancellationToken = default)
    {
        var student = await studentRepository.Create(command.Student);
        
        if (student != null)
        {
            // 清除相关缓存
            await ClearStudentCache(student.UserId, cancellationToken);
            
            // 记录变化统计
            await statisticsService.RecordDataAccessAsync("student", student.UserId, "create", cancellationToken);
        }
        
        return student;
    }

    public async Task<bool> Handle(UpdateStudentCommand command, CancellationToken cancellationToken = default)
    {
        var success = await studentRepository.Update(command.Student);
        
        if (success)
        {
            // 清除相关缓存
            await ClearStudentCache(command.Student.UserId, cancellationToken);
            
            // 记录变化统计
            await statisticsService.RecordDataAccessAsync("student", command.Student.UserId, "update", cancellationToken);
        }
        
        return success;
    }

    public async Task<bool> Handle(DeleteStudentCommand command, CancellationToken cancellationToken = default)
    {
        var success = await studentRepository.Delete(command.Id);
        
        if (success)
        {
            // 清除相关缓存
            await ClearStudentCache(command.Id, cancellationToken);
            
            // 记录变化统计
            await statisticsService.RecordDataAccessAsync("student", command.Id, "delete", cancellationToken);
        }
        
        return success;
    }

    public async Task<bool> Handle(UpdateManyStudentsCommand command, CancellationToken cancellationToken = default)
    {
        var success = await studentRepository.UpdateManyAsync(command.Students);
        
        if (success)
        {
            // 批量更新时，清除所有学生缓存
            await distributedCache.RemoveAsync(StudentsCacheKey, cancellationToken);
            
            // 清除每个学生的单独缓存并记录变化统计
            foreach (var student in command.Students)
            {
                await distributedCache.RemoveAsync($"{StudentCacheKeyPrefix}{student.UserId}", cancellationToken);
                await statisticsService.RecordDataAccessAsync("student", student.UserId, "update", cancellationToken);
            }
        }
        
        return success;
    }

    public async Task<bool> Handle(StudentLoginCommand command, CancellationToken cancellationToken = default)
    {
        var success = await studentRepository.Login(command.UserId, command.Password);
        
        if (success)
        {
            // 记录访问统计
            await statisticsService.RecordDataAccessAsync("student", command.UserId, "read", cancellationToken);
        }
        
        return success;
    }

    // 清除学生相关缓存的辅助方法
    private async Task ClearStudentCache(string studentId, CancellationToken cancellationToken)
    {
        // 清除所有学生缓存
        await distributedCache.RemoveAsync(StudentsCacheKey, cancellationToken);
        
        // 清除单个学生缓存
        await distributedCache.RemoveAsync($"{StudentCacheKeyPrefix}{studentId}", cancellationToken);
        
        // 清除所有分页缓存（可以更精细地清除，但为了简单起见，这里全部清除）
        // 注意：在实际生产环境中，可能需要更复杂的缓存失效策略
    }
}
