using Mapster;
using iOSClub.Data;
using iOSClub.Data.DataObjects;
using iOSClub.Data.DTOs;
using iOSClub.Data.VOs;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.Common;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(
    ITodoRepository todoRepository,
    IStudentRepository studentRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<UserController> logger)
    : ControllerBase
{
    /// <summary>
    /// 获取当前用户的详细信息
    /// </summary>
    /// <returns>用户信息对象</returns>
    [Authorize]
    [HttpGet("data")]
    public async Task<ActionResult<ApiResponse<MemberVO>>> GetData()
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<MemberVO>.Fail(ErrorCode.Unauthorized, "用户未认证"));
            if (member.Identity == "Founder")
                return Ok(ApiResponse<MemberVO>.Success(member, "获取用户信息成功"));

            var student = await studentRepository.GetByIdAsync(member.UserId);
            if (student == null)
                return Ok(ApiResponse<MemberVO>.Fail(ErrorCode.UserNotFound, "用户不存在"));

            var id = member.Identity;
            member = student.Adapt<MemberVO>();
            member.Identity = id;

            return Ok(ApiResponse<MemberVO>.Success(member, "获取用户信息成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "获取用户信息时发生错误");
            }

            return Ok(ApiResponse<MemberVO>.Fail(ErrorCode.InternalServerError, "获取用户信息失败"));
        }
    }

    /// <summary>
    /// 获取当前用户的所有待办事项
    /// </summary>
    /// <returns>待办事项列表</returns>
    [Authorize]
    [HttpGet("todos")]
    public async Task<ActionResult<ApiResponse<List<TodoVO>>>> GetTodos()
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<List<TodoVO>>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            var student = await studentRepository.GetByIdAsync(member.UserId);
            if (student == null)
                return Ok(ApiResponse<List<TodoVO>>.Fail(ErrorCode.UserNotFound, "用户不存在"));

            var todos = await todoRepository.GetTodosByUserIdAsync(student.UserId);
            return Ok(ApiResponse<List<TodoVO>>.Success(todos.Adapt<List<TodoVO>>(), "获取待办事项成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "获取用户待办事项时发生错误");
            }

            return Ok(ApiResponse<List<TodoVO>>.Fail(ErrorCode.InternalServerError, "获取待办事项失败"));
        }
    }

    /// <summary>
    /// 添加新的待办事项
    /// </summary>
    /// <param name="dto">待办事项模型</param>
    /// <returns>添加后的待办事项</returns>
    [Authorize]
    [HttpPost("todos")]
    public async Task<ActionResult<ApiResponse<TodoVO>>> AddTodo([FromBody] TodoCreateUpdateDTO dto)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<TodoVO>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            var student = await studentRepository.GetByIdAsync(member.UserId);
            if (student == null)
                return Ok(ApiResponse<TodoVO>.Fail(ErrorCode.UserNotFound, "用户不存在"));

            var todoModel = dto.Adapt<TodoDO>();
            todoModel.StudentId = student.UserId;
            todoModel.Id = todoModel.ToString();
            todoModel.CreatedTime = DateTime.UtcNow;

            var result = await todoRepository.AddTodoAsync(todoModel);
            if (!result)
                return Ok(ApiResponse<TodoVO>.Fail(ErrorCode.OperationFailed, "添加待办事项失败"));

            return CreatedAtAction(nameof(GetTodoById), new { id = todoModel.Id },
                ApiResponse<TodoVO>.Success(todoModel.Adapt<TodoVO>(), "添加待办事项成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "添加待办事项时发生错误");
            }

            return Ok(ApiResponse<TodoVO>.Fail(ErrorCode.InternalServerError, "添加待办事项失败"));
        }
    }

    /// <summary>
    /// 删除指定ID的待办事项
    /// </summary>
    /// <param name="id">待办事项ID</param>
    /// <returns>操作结果</returns>
    [Authorize]
    [HttpDelete("todos/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteTodo(string id)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<object>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            var hasPermission = await todoRepository.HasPermissionAsync(id, member.UserId);
            if (!hasPermission)
                return Ok(ApiResponse<object>.Fail(ErrorCode.InsufficientPermission, "无权删除此待办事项"));

            var result = await todoRepository.DeleteTodoAsync(id);
            if (!result)
                return Ok(ApiResponse<object>.Fail(ErrorCode.ResourceNotFound, "待办事项不存在或删除失败"));

            return Ok(ApiResponse.Success("待办事项删除成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "删除待办事项时发生错误，ID: {Id}", id);
            }

            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "删除待办事项失败"));
        }
    }

    /// <summary>
    /// 根据ID获取待办事项详情
    /// </summary>
    [HttpGet("todos/{id}")]
    public async Task<ActionResult<ApiResponse<TodoVO>>> GetTodoById(string id)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<TodoVO>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            var todo = await todoRepository.GetTodoByIdAsync(id);
            if (todo == null)
                return Ok(ApiResponse<TodoVO>.Fail(ErrorCode.ResourceNotFound, "待办事项不存在"));

            // 检查权限
            if (todo.StudentId != member.UserId)
                return Ok(ApiResponse<TodoVO>.Fail(ErrorCode.InsufficientPermission, "无权访问此待办事项"));

            return Ok(ApiResponse<TodoVO>.Success(todo.Adapt<TodoVO>(), "获取待办事项详情成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "获取待办事项详情时发生错误，ID: {Id}", id);
            }

            return Ok(ApiResponse<TodoVO>.Fail(ErrorCode.InternalServerError, "获取待办事项详情失败"));
        }
    }

    /// <summary>
    /// 更新待办事项
    /// </summary>
    /// <param name="dto">更新后的待办事项模型</param>
    /// <returns>操作结果</returns>
    [Authorize]
    [HttpPut("todos")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateTodo([FromBody] TodoCreateUpdateDTO dto)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null)
                return Ok(ApiResponse<object>.Fail(ErrorCode.Unauthorized, "用户未认证"));

            var todoModel = dto.Adapt<TodoDO>();

            var hasPermission = await todoRepository.HasPermissionAsync(todoModel.Id, member.UserId);
            if (!hasPermission)
                return Ok(ApiResponse<object>.Fail(ErrorCode.InsufficientPermission, "无权修改此待办事项"));

            var result = await todoRepository.UpdateTodoAsync(todoModel);
            if (!result)
                return Ok(ApiResponse<object>.Fail(ErrorCode.ResourceNotFound, "待办事项不存在或更新失败"));

            return Ok(ApiResponse.Success("待办事项更新成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "更新待办事项时发生错误，ID: {Id}", dto.Id);
            }

            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "更新待办事项失败"));
        }
    }

    /// <summary>
    /// 更新用户个人资料
    /// </summary>
    /// <param name="dto">更新后的用户资料模型</param>
    /// <returns>操作结果</returns>
    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateProfile([FromBody] StudentUpdateDTO dto)
    {
        try
        {
            var member = httpContextAccessor.HttpContext?.User.GetUser();
            if (member == null || member.UserId != dto.UserId)
                return Ok(ApiResponse<object>.Fail(ErrorCode.InsufficientPermission, "权限不足"));

            var result = await studentRepository.UpdateAsync(dto.Adapt<StudentDO>());
            if (!result)
                return Ok(ApiResponse<object>.Fail(ErrorCode.OperationFailed, "更新用户资料失败"));

            return Ok(ApiResponse.Success("更新用户资料成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "更新用户资料时发生错误，用户ID: {UserId}", dto.UserId);
            }

            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "更新用户资料失败"));
        }
    }
}
