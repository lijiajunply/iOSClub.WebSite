using iOSClub.Data;
using iOSClub.Data.DataModels;
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
}