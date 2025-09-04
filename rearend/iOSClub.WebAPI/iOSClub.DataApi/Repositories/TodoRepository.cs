using iOSClub.Data;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

public class TodoRepository
{
    private readonly iOSContext _context;

    public TodoRepository(iOSContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取用户的所有待办事项
    /// </summary>
    public async Task<List<TodoModel>> GetTodosByUserIdAsync(string userId)
    {
        return await _context.Todos
            .Where(x => x.StudentId == userId)
            .OrderByDescending(x => x.CreatedTime)
            .ToListAsync();
    }

    /// <summary>
    /// 根据ID获取待办事项
    /// </summary>
    public async Task<TodoModel?> GetTodoByIdAsync(string id)
    {
        return await _context.Todos
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// 添加待办事项
    /// </summary>
    public async Task<bool> AddTodoAsync(TodoModel todo)
    {
        try
        {
            await _context.Todos.AddAsync(todo);
            await _context.SaveChangesAsync();
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
        try
        {
            var existingTodo = await GetTodoByIdAsync(todo.Id);
            if (existingTodo == null)
                return false;

            existingTodo.Update(todo);
            _context.Todos.Update(existingTodo);
            await _context.SaveChangesAsync();
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
        try
        {
            var todo = await GetTodoByIdAsync(id);
            if (todo == null)
                return false;

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();
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
        return await _context.Todos.AnyAsync(x => x.Id == id);
    }

    /// <summary>
    /// 检查用户是否有权限操作该待办事项
    /// </summary>
    public async Task<bool> HasPermissionAsync(string id, string userId)
    {
        return await _context.Todos
            .AnyAsync(x => x.Id == id && x.StudentId == userId);
    }

    /// <summary>
    /// 获取用户的待办事项数量
    /// </summary>
    public async Task<int> GetTodoCountAsync(string userId)
    {
        return await _context.Todos
            .Where(x => x.StudentId == userId)
            .CountAsync();
    }

    /// <summary>
    /// 获取用户已完成待办事项数量
    /// </summary>
    public async Task<int> GetCompletedTodoCountAsync(string userId)
    {
        return await _context.Todos
            .Where(x => x.StudentId == userId && x.Status)
            .CountAsync();
    }

    /// <summary>
    /// 获取用户待办事项（分页）
    /// </summary>
    public async Task<List<TodoModel>> GetTodosPagedAsync(string userId, int page, int pageSize)
    {
        return await _context.Todos
            .Where(x => x.StudentId == userId)
            .OrderByDescending(x => x.CreatedTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}