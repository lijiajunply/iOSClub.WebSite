using iOSClub.Data;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

public interface ITodoRepository
{
    Task<List<TodoModel>> GetTodosByUserIdAsync(string userId);
    Task<TodoModel?> GetTodoByIdAsync(string id);
    Task<bool> AddTodoAsync(TodoModel todo);
    Task<bool> UpdateTodoAsync(TodoModel todo);
    Task<bool> DeleteTodoAsync(string id);
    Task<bool> TodoExistsAsync(string id);

    Task<bool> HasPermissionAsync(string id, string userId);

    Task<int> GetTodoCountAsync(string userId);

    Task<int> GetCompletedTodoCountAsync(string userId);

    Task<List<TodoModel>> GetTodosPagedAsync(string userId, int page, int pageSize);
}

public class TodoRepository(IDbContextFactory<ClubContext> factory) : ITodoRepository
{

    /// <summary>
    /// 获取用户的所有待办事项
    /// </summary>
    public async Task<List<TodoModel>> GetTodosByUserIdAsync(string userId)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Todos
            .Where(x => x.StudentId == userId)
            .OrderByDescending(x => x.CreatedTime)
            .ToListAsync();
    }

    /// <summary>
    /// 根据ID获取待办事项
    /// </summary>
    public async Task<TodoModel?> GetTodoByIdAsync(string id)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Todos
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// 添加待办事项
    /// </summary>
    public async Task<bool> AddTodoAsync(TodoModel todo)
    {
        await using var context = await factory.CreateDbContextAsync();
        try
        {
            await context.Todos.AddAsync(todo);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 更新待办事项
    /// </summary>
    public async Task<bool> UpdateTodoAsync(TodoModel todo)
    {
        await using var context = await factory.CreateDbContextAsync();
        try
        {
            var existingTodo = await context.Todos.FirstOrDefaultAsync(x => x.Id == todo.Id);
            if (existingTodo == null)
                return false;

            existingTodo.Update(todo);
            context.Todos.Update(existingTodo);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 删除待办事项
    /// </summary>
    public async Task<bool> DeleteTodoAsync(string id)
    {
        await using var context = await factory.CreateDbContextAsync();
        try
        {
            var todo = await context.Todos.FirstOrDefaultAsync(x => x.Id == id);
            if (todo == null)
                return false;

            context.Todos.Remove(todo);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查待办事项是否存在
    /// </summary>
    public async Task<bool> TodoExistsAsync(string id)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Todos.AnyAsync(x => x.Id == id);
    }

    /// <summary>
    /// 检查用户是否有权限操作该待办事项
    /// </summary>
    public async Task<bool> HasPermissionAsync(string id, string userId)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Todos
            .AnyAsync(x => x.Id == id && x.StudentId == userId);
    }

    /// <summary>
    /// 获取用户的待办事项数量
    /// </summary>
    public async Task<int> GetTodoCountAsync(string userId)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Todos
            .Where(x => x.StudentId == userId)
            .CountAsync();
    }

    /// <summary>
    /// 获取用户已完成待办事项数量
    /// </summary>
    public async Task<int> GetCompletedTodoCountAsync(string userId)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Todos
            .Where(x => x.StudentId == userId && x.Status)
            .CountAsync();
    }

    /// <summary>
    /// 获取用户待办事项（分页）
    /// </summary>
    public async Task<List<TodoModel>> GetTodosPagedAsync(string userId, int page, int pageSize)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Todos
            .Where(x => x.StudentId == userId)
            .OrderByDescending(x => x.CreatedTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}