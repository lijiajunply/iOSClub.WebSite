using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.Data.ShowModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

public interface IStudentRepository
{
    public Task<List<StudentModel>> GetAll();
    public Task<StudentModel?> Get(string id);
    public Task<StudentModel?> Create(StudentModel model);
    public Task<bool> Update(StudentModel model);
    public Task<bool> Delete(string id);
    public Task<bool> Login(string userId, string password);
    
    // 新增方法
    public Task<StudentModel?> GetByIdAsync(string id);
    public Task<bool> UpdateAsync(MemberModel model);
    public Task<bool> DeleteAsync(string id);
    public Task<List<StudentModel>> UpdateManyAsync(List<StudentModel> list);
    public Task<List<MemberModel>> GetAllMembersAsync();
    public Task<(List<MemberModel>, int)> GetMembersPagedAsync(int pageNum, int pageSize);
}

public class StudentRepository(iOSContext context) : IStudentRepository
{
    public async Task<List<StudentModel>> GetAll()
    {
        var students = await context.Students.ToListAsync();
        return students;
    }

    public async Task<StudentModel?> Get(string id)
    {
        var stu = await context.Students.FirstOrDefaultAsync(x => x.UserId == id);
        return stu;
    }

    public async Task<bool> Update(StudentModel model)
    {
        var stu = await Get(model.UserId);
        if (stu == null) return false;
        stu.Update(model);
        return await context.SaveChangesAsync() == 1;
    }

    public async Task<StudentModel?> Create(StudentModel model)
    {
        var stu = await Get(model.UserId);
        if (stu != null) return null;

        context.Students.Add(model);
        return await context.SaveChangesAsync() == 1 ? model : null;
    }

    public async Task<bool> Delete(string id)
    {
        var stu = await Get(id);
        if (stu == null) return false;
        context.Students.Remove(stu);
        return await context.SaveChangesAsync() == 1;
    }

    public async Task<bool> Login(string userId, string password)
    {
        var hash = DataTool.StringToHash(password);

        return await context.Students.AnyAsync(x =>
            x.UserId == userId && (string.IsNullOrEmpty(x.PasswordHash)
                ? password == x.PhoneNum
                : hash == x.PasswordHash));
    }
    
    // 新增方法实现
    public async Task<StudentModel?> GetByIdAsync(string id)
    {
        return await context.Students.FirstOrDefaultAsync(x => x.UserId == id);
    }

    public async Task<bool> UpdateAsync(MemberModel model)
    {
        var stu = await GetByIdAsync(model.UserId);
        if (stu == null) return false;
        stu.Update(model);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var stu = await GetByIdAsync(id);
        if (stu == null) return false;
        context.Students.Remove(stu);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<List<StudentModel>> UpdateManyAsync(List<StudentModel> list)
    {
        // 获取所有现有的学生ID
        var existingStudentIds = await context.Students
            .Select(s => s.UserId)
            .ToHashSetAsync();

        // 过滤出只需要添加的学生
        var newStudents = list
            .Where(model => !existingStudentIds.Contains(model.UserId))
            .Select(model => model.Standardization())
            .ToList();

        // 批量添加新学生
        if (newStudents.Count > 0)
        {
            await context.Students.AddRangeAsync(newStudents);
            await context.SaveChangesAsync();
        }

        return await context.Students.ToListAsync();
    }

    public async Task<List<MemberModel>> GetAllMembersAsync()
    {
        var query = from student in context.Students
                   join staff in context.Staffs
                       on student.UserId equals staff.UserId into staffGroup
                   from staff in staffGroup.DefaultIfEmpty() // LEFT JOIN
                   select MemberModel.CopyFrom(student, staff != null ? staff.Identity : "Member");
        
        return await query.ToListAsync();
    }

    public async Task<(List<MemberModel>, int)> GetMembersPagedAsync(int pageNum, int pageSize)
    {
        var skipCount = (pageNum - 1) * pageSize;
        var studentIdsQuery = context.Students
            .OrderBy(s => s.UserId) // 确保结果一致性的排序
            .Skip(skipCount)
            .Take(pageSize)
            .Select(s => s.UserId);

        var studentIds = await studentIdsQuery.ToListAsync(); // 使用两个更小的查询代替一个复杂查询
        var studentsQuery = context.Students
            .Where(s => studentIds.Contains(s.UserId))
            .AsNoTracking(); // 禁用变更跟踪提高性能
        var staffQuery = context.Staffs
            .Where(s => studentIds.Contains(s.UserId))
            .AsNoTracking(); // 并行执行两个查询

        var totalCount = await context.Students.CountAsync();
        var students = await studentsQuery.ToListAsync();
        var staffs = await staffQuery.ToListAsync();

        // 在内存中执行连接操作
        var staffMap = staffs.ToDictionary(s => s.UserId);

        var results = students.Select(student =>
        {
            staffMap.TryGetValue(student.UserId, out var staff);
            return MemberModel.CopyFrom(student, staff != null ? staff.Identity : "Member");
        }).ToList();

        return (results, totalCount);
    }
}