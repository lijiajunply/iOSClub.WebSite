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

public class StudentRepository(IDbContextFactory<ClubContext> factory) : IStudentRepository
{
    // 使用EF Core编译查询，缓存查询计划，提高重复查询性能
    private static readonly Func<ClubContext, string, Task<StudentModel?>> GetStudentByIdQuery =
        EF.CompileAsyncQuery((ClubContext context, string id) =>
            context.Students.AsNoTracking().FirstOrDefault(s => s.UserId == id));

    private static readonly Func<ClubContext, string, string, Task<StudentModel?>> LoginQuery =
        EF.CompileAsyncQuery((ClubContext context, string userId, string passwordHash) =>
            context.Students.AsNoTracking()
                .FirstOrDefault(s => s.UserId == userId && (string.IsNullOrEmpty(s.PasswordHash)
                    ? passwordHash == s.PhoneNum
                    : s.PasswordHash == passwordHash)));

    public async Task<List<StudentModel>> GetAll()
    {
        await using var context = await factory.CreateDbContextAsync();
        var students = await context.Students.AsNoTracking().ToListAsync();
        return students;
    }

    public async Task<StudentModel?> Get(string id)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await GetStudentByIdQuery(context, id);
    }

    public async Task<StudentModel?> GetByIdAsync(string id)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await GetStudentByIdQuery(context, id);
    }

    public async Task<bool> Update(StudentModel model)
    {
        await using var context = await factory.CreateDbContextAsync();
        var stu = await context.Students.FirstOrDefaultAsync(x => x.UserId == model.UserId);
        if (stu == null) return false;
        stu.Update(model);
        return await context.SaveChangesAsync() == 1;
    }

    public async Task<StudentModel?> Create(StudentModel model)
    {
        await using var context = await factory.CreateDbContextAsync();
        var stu = await context.Students.FirstOrDefaultAsync(x => x.UserId == model.UserId);
        if (stu != null) return null;

        await context.Students.AddAsync(model);
        return await context.SaveChangesAsync() == 1 ? model : null;
    }

    public async Task<bool> Delete(string id)
    {
        await using var context = await factory.CreateDbContextAsync();
        var stu = await context.Students.FirstOrDefaultAsync(x => x.UserId == id);
        if (stu == null) return false;
        context.Students.Remove(stu);
        return await context.SaveChangesAsync() == 1;
    }

    public async Task<bool> Login(string userId, string password)
    {
        await using var context = await factory.CreateDbContextAsync();
        var hash = DataTool.StringToHash(password);

        // 使用编译查询提高性能
        var student = await LoginQuery(context, userId, hash);
        return student != null;
    }



    public async Task<bool> UpdateAsync(StudentModel model)
    {
        await using var context = await factory.CreateDbContextAsync();
        var stu = await context.Students.FirstOrDefaultAsync(x => x.UserId == model.UserId);
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
        await using var context = await factory.CreateDbContextAsync();
        var stu = await context.Students.FirstOrDefaultAsync(x => x.UserId == id);
        if (stu == null) return false;
        context.Students.Remove(stu);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateManyAsync(List<StudentModel> list)
    {
        await using var context = await factory.CreateDbContextAsync();
        // 获取所有现有的学生ID
        var existingStudentIds = await context.Students
            .Select(s => s.UserId)
            .ToHashSetAsync();

        // 标准化所有学生数据
        var standardizedStudents = list.Select(model => model.Standardization()).ToList();

        // 分离需要插入和更新的学生
        var newStudents = standardizedStudents
            .Where(model => !existingStudentIds.Contains(model.UserId))
            .ToList();

        var existingStudents = standardizedStudents
            .Where(model => existingStudentIds.Contains(model.UserId))
            .ToList();

        // 批量操作：使用EF Core 7.0+的高效批量处理
        var changes = false;

        // 批量添加新学生
        if (newStudents.Count > 0)
        {
            await context.Students.AddRangeAsync(newStudents);
            changes = true;
        }

        // 批量更新现有学生：使用ExecuteUpdateAsync进行高效的批量更新
        if (existingStudents.Count > 0)
        {
            // 对于EF Core 7.0+，可以使用ExecuteUpdateAsync进行批量更新
            // 这里为每个学生单独更新，因为需要调用Update方法处理复杂的更新逻辑
            // 在实际应用中，可以根据具体情况选择更高效的批量更新方式
            foreach (var student in existingStudents)
            {
                var existingStudent = await context.Students.FirstOrDefaultAsync(s => s.UserId == student.UserId);
                if (existingStudent != null)
                {
                    existingStudent.Update(student);
                    changes = true;
                }
            }
        }

        // 只有在有变化时才调用SaveChangesAsync，减少数据库调用
        return changes ? await context.SaveChangesAsync() > 0 : true;
    }

    public async Task<List<MemberModel>> GetAllMembersAsync()
    {
        await using var context = await factory.CreateDbContextAsync();
        var query = from student in context.Students.AsNoTracking()
            join staff in context.Staffs.AsNoTracking()
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
        await using var context = await factory.CreateDbContextAsync();
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

        // 计算总记录数
        var totalCount = await query.CountAsync();
        var skipCount = (pageNum - 1) * pageSize;

        // 在数据库层面执行LEFT JOIN操作，避免内存中连接
        var memberQuery = from student in query.AsNoTracking()
            join staff in context.Staffs.AsNoTracking()
                on student.UserId equals staff.UserId into staffGroup
            from staff in staffGroup.DefaultIfEmpty() // LEFT JOIN
            orderby student.UserId // 确保结果一致性的排序
            select new
            {
                Student = student,
                StaffIdentity = staff != null ? staff.Identity : "Member"
            };

        // 应用分页并执行查询
        var memberData = await memberQuery
            .Skip(skipCount)
            .Take(pageSize)
            .ToListAsync();

        // 转换为MemberModel列表
        var results = memberData.Select(item =>
            MemberModel.CopyFrom(item.Student, item.StaffIdentity)).ToList();

        return (results, totalCount);
    }

    public async Task<List<StudentModel>> Search(string searchTerm, string searchCondition)
    {
        await using var context = await factory.CreateDbContextAsync();
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

        // 直接返回结果，避免多余的查询和内存操作
        return await query.AsNoTracking()
            .OrderBy(s => s.UserId) // 确保结果一致性
            .ToListAsync();
    }
}