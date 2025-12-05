using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.Common;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iOSClub.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]  // 使用C#推荐的API路径格式
public class TodoController(
    ITodoRepository todoRepository,
    IStudentRepository studentRepository,
    IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    /// <summary>
    /// 获取当前用户的所有待办事项
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TodoModel>>>> GetTodos()
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<List<TodoModel>>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            var student = await studentRepository.GetByIdAsync(member.UserId);
            if (student == null)
                return Ok(ApiResponse<List<TodoModel>>.Fail(ErrorCode.UserNotFound, "学生信息不存在"));

            var todos = await todoRepository.GetTodosByUserIdAsync(student.UserId);
            return Ok(ApiResponse<List<TodoModel>>.Success(todos, "获取待办事项成功"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<List<TodoModel>>.Fail(ErrorCode.InternalServerError, "获取待办事项失败"));
        }
    }

    /// <summary>
    /// 获取待办事项统计信息
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<object>>> GetTodoStatistics()
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<object>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            var total = await todoRepository.GetTodoCountAsync(member.UserId);
            var completed = await todoRepository.GetCompletedTodoCountAsync(member.UserId);
            var pending = total - completed;

            var result = new
            {
                Total = total,
                Completed = completed,
                Pending = pending,
                CompletionRate = total > 0 ? (double)completed / total * 100 : 0
            };

            return Ok(ApiResponse<object>.Success(result, "获取待办事项统计成功"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "获取待办事项统计失败"));
        }
    }

    /// <summary>
    /// 根据ID获取待办事项详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TodoModel>>> GetTodoById(string id)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<TodoModel>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            var todo = await todoRepository.GetTodoByIdAsync(id);
            if (todo == null)
                return Ok(ApiResponse<TodoModel>.Fail(ErrorCode.ResourceNotFound, "待办事项不存在"));

            // 检查权限
            if (todo.StudentId != member.UserId)
                return Ok(ApiResponse<TodoModel>.Fail(ErrorCode.InsufficientPermission, "无权访问此待办事项"));

            return Ok(ApiResponse<TodoModel>.Success(todo, "获取待办事项详情成功"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<TodoModel>.Fail(ErrorCode.InternalServerError, "获取待办事项详情失败"));
        }
    }

    /// <summary>
    /// 添加新的待办事项
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<string>>> AddTodo(TodoModel todoModel)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<string>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            var student = await studentRepository.GetByIdAsync(member.UserId);
            if (student == null)
                return Ok(ApiResponse<string>.Fail(ErrorCode.UserNotFound, "学生信息不存在"));

            // 设置待办事项属性
            todoModel.StudentId = student.UserId;
            todoModel.Id = todoModel.ToHash();
            todoModel.CreatedTime = DateTime.UtcNow;

            var result = await todoRepository.AddTodoAsync(todoModel);
            if (!result)
                return Ok(ApiResponse<string>.Fail(ErrorCode.OperationFailed, "添加待办事项失败"));

            return CreatedAtAction(nameof(GetTodoById), new { id = todoModel.Id }, ApiResponse<string>.Success(todoModel.Id, "添加待办事项成功"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<string>.Fail(ErrorCode.InternalServerError, "添加待办事项失败"));
        }
    }

    /// <summary>
    /// 更新待办事项
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<ApiResponse<object>>> UpdateTodo(TodoModel todoModel)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<object>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            // 检查待办事项是否存在且用户有权限
            var hasPermission = await todoRepository.HasPermissionAsync(todoModel.Id, member.UserId);
            if (!hasPermission)
                return Ok(ApiResponse<object>.Fail(ErrorCode.InsufficientPermission, "无权修改此待办事项"));

            var result = await todoRepository.UpdateTodoAsync(todoModel);
            if (!result)
                return Ok(ApiResponse<object>.Fail(ErrorCode.ResourceNotFound, "待办事项不存在或更新失败"));

            return Ok(ApiResponse<object>.Success(null, "待办事项更新成功"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "更新待办事项失败"));
        }
    }

    /// <summary>
    /// 删除待办事项
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteTodo(string id)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<object>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            // 检查待办事项是否存在且用户有权限
            var hasPermission = await todoRepository.HasPermissionAsync(id, member.UserId);
            if (!hasPermission)
                return Ok(ApiResponse<object>.Fail(ErrorCode.InsufficientPermission, "无权删除此待办事项"));

            var result = await todoRepository.DeleteTodoAsync(id);
            if (!result)
                return Ok(ApiResponse<object>.Fail(ErrorCode.ResourceNotFound, "待办事项不存在或删除失败"));

            return Ok(ApiResponse<object>.Success(null, "待办事项删除成功"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "删除待办事项失败"));
        }
    }

    /// <summary>
    /// 获取分页待办事项
    /// </summary>
    /// <param name="page">页码，从1开始</param>
    /// <param name="pageSize">每页大小，范围1-50</param>
    /// <returns>分页的待办事项列表</returns>
    [HttpGet("Page/{page}/{pageSize}")]
    public async Task<ActionResult<ApiResponse<List<TodoModel>>>> GetTodosPaged(int page = 1, int pageSize = 10)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<List<TodoModel>>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var todos = await todoRepository.GetTodosPagedAsync(member.UserId, page, pageSize);
            return Ok(ApiResponse<List<TodoModel>>.Success(todos, "获取分页待办事项成功"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<List<TodoModel>>.Fail(ErrorCode.InternalServerError, "获取分页待办事项失败"));
        }
    }
}