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
    public Task<bool> UpdateAsync(StudentModel model);
    public Task<bool> DeleteAsync(string id);
    public Task<bool> UpdateManyAsync(List<StudentModel> list);
    public Task<List<MemberModel>> GetAllMembersAsync();
    public Task<(List<MemberModel>, int)> GetMembersPagedAsync(int pageNum, int pageSize);

    // 带搜索功能的分页方法
    public Task<(List<MemberModel>, int)> GetMembersPagedAsync(int pageNum, int pageSize, string? searchTerm,
        string? searchCondition);

    public Task<List<StudentModel>> Search(string searchTerm, string searchCondition);
}

public class StudentRepository(ClubContext context) : IStudentRepository
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

    public async Task<bool> UpdateAsync(StudentModel model)
    {
        var stu = await GetByIdAsync(model.UserId);
        if (stu == null)
        {
            model.Standardization();
            await context.Students.AddAsync(model);
        }
        else
        {
            stu.Update(model);
        }

        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var stu = await GetByIdAsync(id);
        if (stu == null) return false;
        context.Students.Remove(stu);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateManyAsync(List<StudentModel> list)
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
            return await context.SaveChangesAsync() > 0;
        }

        return true;
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
        // 调用带搜索参数的版本，传递null值表示无搜索条件
        return await GetMembersPagedAsync(pageNum, pageSize, null, null);
    }

    // 带搜索功能的分页方法实现
    public async Task<(List<MemberModel>, int)> GetMembersPagedAsync(int pageNum, int pageSize, string? searchTerm,
        string? searchCondition)
    {
        // 构建基础查询
        var query = context.Students.AsQueryable();

        // 如果提供了搜索词，则应用搜索条件
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = searchCondition?.ToLower() switch
            {
                "userid" => query.Where(s => s.UserId.StartsWith(searchTerm)),
                "username" => query.Where(s => s.UserName.Contains(searchTerm)),
                "classname" => query.Where(s => s.ClassName.Contains(searchTerm)),
                "academy" => query.Where(s => s.Academy.Contains(searchTerm)),
                "phone_num" => query.Where(s => s.PhoneNum.Contains(searchTerm)),
                _ => query.Where(s =>
                    s.UserId.Contains(searchTerm) || s.UserName.Contains(searchTerm) ||
                    s.ClassName.Contains(searchTerm) || s.Academy.Contains(searchTerm) ||
                    s.PhoneNum.Contains(searchTerm))
            };
        }

        var skipCount = (pageNum - 1) * pageSize;
        var studentIdsQuery = query
            .OrderBy(s => s.UserId) // 确保结果一致性的排序
            .Skip(skipCount)
            .Take(pageSize)
            .Select(s => s.UserId);

        var studentIds = await studentIdsQuery.ToListAsync();
        var studentsQuery = context.Students
            .Where(s => studentIds.Contains(s.UserId))
            .AsNoTracking();
        var staffQuery = context.Staffs
            .Where(s => studentIds.Contains(s.UserId))
            .AsNoTracking();

        var totalCount = await query.CountAsync();
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

    public async Task<List<StudentModel>> Search(string searchTerm, string searchCondition)
    {
        var query = context.Students.AsQueryable();

        // 如果提供了搜索词，则应用搜索条件
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = searchCondition.ToLower() switch
            {
                "userid" => query.Where(s => s.UserId.StartsWith(searchTerm)),
                "username" => query.Where(s => s.UserName.Contains(searchTerm)),
                "classname" => query.Where(s => s.ClassName.Contains(searchTerm)),
                "academy" => query.Where(s => s.Academy.Contains(searchTerm)),
                "phone_num" => query.Where(s => s.PhoneNum.Contains(searchTerm)),
                _ => query.Where(s =>
                    s.UserId.Contains(searchTerm) || s.UserName.Contains(searchTerm) ||
                    s.ClassName.Contains(searchTerm) || s.Academy.Contains(searchTerm) ||
                    s.PhoneNum.Contains(searchTerm))
            };
        }


        var studentIdsQuery = query
            .OrderBy(s => s.UserId) // 确保结
            .Select(s => s.UserId);

        var studentIds = await studentIdsQuery.ToListAsync();
        var studentsQuery = context.Students
            .Where(s => studentIds.Contains(s.UserId))
            .AsNoTracking();

        var students = await studentsQuery.ToListAsync();

        return students;
    }
}