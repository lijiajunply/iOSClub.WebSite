using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.Data.ShowModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

/// <summary>
/// 学生仓库接口，提供学生数据的CRUD操作和查询功能
/// </summary>
public interface IStudentRepository
{
    /// <summary>
    /// 获取所有学生
    /// </summary>
    /// <returns>学生列表</returns>
    public Task<List<StudentModel>> GetAll();
    
    /// <summary>
    /// 根据ID获取学生
    /// </summary>
    /// <param name="id">学生ID</param>
    /// <returns>学生模型，如果找不到则返回null</returns>
    public Task<StudentModel?> Get(string id) => GetByIdAsync(id);
    
    /// <summary>
    /// 创建学生
    /// </summary>
    /// <param name="model">学生模型</param>
    /// <returns>创建的学生模型，如果创建失败则返回null</returns>
    /// <exception cref="ArgumentException">当输入参数无效时抛出</exception>
    public Task<StudentModel?> Create(StudentModel model);
    
    /// <summary>
    /// 更新学生
    /// </summary>
    /// <param name="model">学生模型</param>
    /// <returns>是否更新成功</returns>
    public Task<bool> Update(StudentModel model);
    
    /// <summary>
    /// 删除学生
    /// </summary>
    /// <param name="id">学生ID</param>
    /// <returns>是否删除成功</returns>
    public Task<bool> Delete(string id) => DeleteAsync(id);
    
    /// <summary>
    /// 学生登录验证
    /// </summary>
    /// <param name="userId">学生ID</param>
    /// <param name="password">密码</param>
    /// <returns>是否登录成功</returns>
    public Task<bool> Login(string userId, string password);

    /// <summary>
    /// 根据ID异步获取学生
    /// </summary>
    /// <param name="id">学生ID</param>
    /// <returns>学生模型，如果找不到则返回null</returns>
    public Task<StudentModel?> GetByIdAsync(string id);
    
    /// <summary>
    /// 异步更新学生
    /// </summary>
    /// <param name="model">学生模型</param>
    /// <returns>是否更新成功</returns>
    public Task<bool> UpdateAsync(StudentModel model);
    
    /// <summary>
    /// 异步删除学生
    /// </summary>
    /// <param name="id">学生ID</param>
    /// <returns>是否删除成功</returns>
    public Task<bool> DeleteAsync(string id);
    
    /// <summary>
    /// 异步批量更新学生
    /// </summary>
    /// <param name="list">学生列表</param>
    /// <returns>是否更新成功</returns>
    public Task<bool> UpdateManyAsync(List<StudentModel> list);
    
    /// <summary>
    /// 异步获取所有成员
    /// </summary>
    /// <returns>成员列表</returns>
    public Task<List<MemberModel>> GetAllMembersAsync();
    
    /// <summary>
    /// 异步分页获取成员
    /// </summary>
    /// <param name="pageNum">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>成员列表和总记录数</returns>
    public Task<(List<MemberModel>, int)> GetMembersPagedAsync(int pageNum, int pageSize);

    /// <summary>
    /// 带搜索功能的异步分页获取成员
    /// </summary>
    /// <param name="pageNum">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="searchTerm">搜索词</param>
    /// <param name="searchCondition">搜索条件</param>
    /// <returns>成员列表和总记录数</returns>
    public Task<(List<MemberModel>, int)> GetMembersPagedAsync(int pageNum, int pageSize, string? searchTerm,
        string? searchCondition);

    /// <summary>
    /// 搜索学生
    /// </summary>
    /// <param name="searchTerm">搜索词</param>
    /// <param name="searchCondition">搜索条件</param>
    /// <returns>学生列表</returns>
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

    public async Task<StudentModel?> GetByIdAsync(string id)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await GetStudentByIdQuery(context, id);
    }

    public async Task<bool> Update(StudentModel model)
    {
        // 输入验证
        if (string.IsNullOrWhiteSpace(model.UserId))
        {
            return false;
        }

        // 验证手机号格式
        if (!string.IsNullOrWhiteSpace(model.PhoneNum) && !ValidationTool.IsValidPhoneNumber(model.PhoneNum))
        {
            return false;
        }

        // 验证邮箱格式
        if (!string.IsNullOrEmpty(model.EMail) && !ValidationTool.IsValidEmail(model.EMail))
        {
            return false;
        }

        await using var context = await factory.CreateDbContextAsync();
        var stu = await context.Students.FirstOrDefaultAsync(x => x.UserId == model.UserId);
        if (stu == null)
        {
            return false;
        }

        stu.Update(model);
        var result = await context.SaveChangesAsync();

        return result == 1;
    }

    public async Task<StudentModel?> Create(StudentModel model)
    {
        // 输入验证
        if (string.IsNullOrWhiteSpace(model.UserId))
        {
            throw new ArgumentException("用户ID不能为空");
        }

        if (string.IsNullOrWhiteSpace(model.UserName))
        {
            throw new ArgumentException("用户名不能为空");
        }

        // 验证手机号格式
        if (!ValidationTool.IsValidPhoneNumber(model.PhoneNum))
        {
            throw new ArgumentException("手机号格式错误");
        }

        // 验证邮箱格式
        if (!string.IsNullOrEmpty(model.EMail) && !ValidationTool.IsValidEmail(model.EMail))
        {
            throw new ArgumentException("邮箱格式错误");
        }

        //如果密码不是哈希加密过的，则进行加密
        if (!string.IsNullOrEmpty(model.PasswordHash) && !DataTool.IsValidHash(model.PasswordHash))
        {
            model.PasswordHash = DataTool.StringToHash(model.PasswordHash);
        }

        await using var context = await factory.CreateDbContextAsync();
        var stu = await context.Students.FirstOrDefaultAsync(x => x.UserId == model.UserId);
        if (stu != null)
        {
            return null;
        }

        await context.Students.AddAsync(model);
        var result = await context.SaveChangesAsync();

        return result == 1 ? model : null;
    }

    public async Task<bool> Login(string userId, string password)
    {
        // 输入验证
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        await using var context = await factory.CreateDbContextAsync();
        var hash = DataTool.StringToHash(password);

        // 使用编译查询提高性能
        var student = await LoginQuery(context, userId, hash);

        return student != null;
    }


    public async Task<bool> UpdateAsync(StudentModel model)
    {
        // 输入验证
        if (string.IsNullOrWhiteSpace(model.UserId))
        {
            return false;
        }

        // 验证手机号格式
        if (!string.IsNullOrWhiteSpace(model.PhoneNum) && !ValidationTool.IsValidPhoneNumber(model.PhoneNum))
        {
            return false;
        }

        // 验证邮箱格式
        if (!string.IsNullOrEmpty(model.EMail) && !ValidationTool.IsValidEmail(model.EMail))
        {
            return false;
        }

        await using var context = await factory.CreateDbContextAsync();
        var stu = await context.Students.FirstOrDefaultAsync(x => x.UserId == model.UserId);
        if (stu == null)
        {
            model.Standardization();

            // 确保PhoneNum作为默认密码
            if (string.IsNullOrWhiteSpace(model.PasswordHash))
            {
                model.PasswordHash = DataTool.StringToHash(model.PhoneNum);
            }

            await context.Students.AddAsync(model);
        }
        else
        {
            stu.Update(model);
        }

        var result = await context.SaveChangesAsync();

        return result > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        // 输入验证
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        await using var context = await factory.CreateDbContextAsync();
        var stu = await context.Students.FirstOrDefaultAsync(x => x.UserId == id);
        if (stu == null)
        {
            return false;
        }

        context.Students.Remove(stu);
        var result = await context.SaveChangesAsync();

        return result > 0;
    }

    public async Task<bool> UpdateManyAsync(List<StudentModel> list)
    {
        // 输入验证
        if (list.Count == 0)
        {
            return true; // 返回true表示没有需要更新的内容
        }

        // 验证列表中的每个学生数据
        var validStudents = list.Where(model => !string.IsNullOrWhiteSpace(model.UserId))
            .Where(model =>
                !string.IsNullOrWhiteSpace(model.PhoneNum) && ValidationTool.IsValidPhoneNumber(model.PhoneNum));

        await using var context = await factory.CreateDbContextAsync();
        // 获取所有现有的学生ID
        var existingStudentIds = await context.Students
            .Select(s => s.UserId)
            .ToHashSetAsync();

        // 标准化所有学生数据
        var standardizedStudents = validStudents.Select(model => model.Standardization()).ToList();

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
            // 为新学生设置默认密码
            foreach (var student in newStudents.Where(student => string.IsNullOrWhiteSpace(student.PasswordHash)))
            {
                student.PasswordHash = DataTool.StringToHash(student.PhoneNum);
            }

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
                if (existingStudent == null) continue;
                existingStudent.Update(student);
                changes = true;
            }
        }

        // 只有在有变化时才调用SaveChangesAsync，减少数据库调用
        if (!changes) return true;
        var result = await context.SaveChangesAsync();
        return result > 0;
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