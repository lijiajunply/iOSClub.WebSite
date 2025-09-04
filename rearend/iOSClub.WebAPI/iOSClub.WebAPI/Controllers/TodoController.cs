using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.WebAPI.Controllers;

[Authorize]
[TokenActionFilter]
[Route("[controller]/[action]")]
[ApiController]
public class TodoController(
    IDbContextFactory<iOSContext> factory,
    IHttpContextAccessor httpContextAccessor,
    TodoRepository todoRepository)
    : ControllerBase
{
    /// <summary>
    /// 获取当前用户的所有待办事项
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<TodoModel>>> GetTodos()
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Unauthorized("用户未认证");

            await using var context = await factory.CreateDbContextAsync();
            var student = await context.Students.FirstOrDefaultAsync(x => x.UserId == member.UserId);
            if (student == null)
                return NotFound("学生信息不存在");

            var todos = await todoRepository.GetTodosByUserIdAsync(student.UserId);
            return Ok(todos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取待办事项统计信息
    /// </summary>
    [HttpGet("Statistics")]
    public async Task<ActionResult<object>> GetTodoStatistics()
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Unauthorized("用户未认证");

            var total = await todoRepository.GetTodoCountAsync(member.UserId);
            var completed = await todoRepository.GetCompletedTodoCountAsync(member.UserId);
            var pending = total - completed;

            return Ok(new
            {
                Total = total,
                Completed = completed,
                Pending = pending,
                CompletionRate = total > 0 ? (double)completed / total * 100 : 0
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取待办事项详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoModel>> GetTodoById(string id)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Unauthorized("用户未认证");

            var todo = await todoRepository.GetTodoByIdAsync(id);
            if (todo == null)
                return NotFound("待办事项不存在");

            // 检查权限
            if (todo.StudentId != member.UserId)
                return Forbid("无权访问此待办事项");

            return Ok(todo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 添加新的待办事项
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<string>> AddTodo(TodoModel todoModel)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Unauthorized("用户未认证");

            await using var context = await factory.CreateDbContextAsync();
            var student = await context.Students.FirstOrDefaultAsync(x => x.UserId == member.UserId);
            if (student == null)
                return NotFound("学生信息不存在");

            // 设置待办事项属性
            todoModel.StudentId = student.UserId;
            todoModel.Id = todoModel.ToHash();
            todoModel.CreatedTime = DateTime.Now;

            var result = await todoRepository.AddTodoAsync(todoModel);
            if (!result)
                return BadRequest("添加待办事项失败");

            return CreatedAtAction(nameof(GetTodoById), new { id = todoModel.Id }, todoModel.Id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新待办事项
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateTodo(TodoModel todoModel)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Unauthorized("用户未认证");

            // 检查待办事项是否存在且用户有权限
            var hasPermission = await todoRepository.HasPermissionAsync(todoModel.Id, member.UserId);
            if (!hasPermission)
                return Forbid("无权修改此待办事项");

            var result = await todoRepository.UpdateTodoAsync(todoModel);
            if (!result)
                return NotFound("待办事项不存在或更新失败");

            return Ok("待办事项更新成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除待办事项
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(string id)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Unauthorized("用户未认证");

            // 检查待办事项是否存在且用户有权限
            var hasPermission = await todoRepository.HasPermissionAsync(id, member.UserId);
            if (!hasPermission)
                return Forbid("无权删除此待办事项");

            var result = await todoRepository.DeleteTodoAsync(id);
            if (!result)
                return NotFound("待办事项不存在或删除失败");

            return Ok("待办事项删除成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取分页待办事项
    /// </summary>
    [HttpGet("Page/{page}/{pageSize}")]
    public async Task<ActionResult<List<TodoModel>>> GetTodosPaged(int page = 1, int pageSize = 10)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Unauthorized("用户未认证");

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var todos = await todoRepository.GetTodosPagedAsync(member.UserId, page, pageSize);
            return Ok(todos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }
}