using iOSClub.Data;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoModel = iOSClub.Data.DataModels.TodoModel;
using MemberModel = iOSClub.Data.ShowModels.MemberModel;
using StudentModel = iOSClub.Data.DataModels.StudentModel;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(
    ITodoRepository todoRepository,
    IStudentRepository studentRepository,
    IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    /// <summary>
    /// 获取当前用户的详细信息
    /// </summary>
    /// <returns>用户信息对象</returns>
    [TokenActionFilter]
    [Authorize]
    [HttpGet("data")]
    public async Task<ActionResult<MemberModel>> GetData()
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();
        if (member.Identity == "Founder") return member;

        var student = await studentRepository.GetByIdAsync(member.UserId);
        if (student == null) return NotFound();

        var id = member.Identity;
        member = MemberModel.AutoCopy<StudentModel, MemberModel>(student);
        member.Identity = id;

        return member;
    }

    /// <summary>
    /// 获取当前用户的所有待办事项
    /// </summary>
    /// <returns>待办事项列表</returns>
    [TokenActionFilter]
    [Authorize]
    [HttpGet("todos")]
    public async Task<ActionResult<List<TodoModel>>> GetTodos()
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();

        var student = await studentRepository.GetByIdAsync(member.UserId);
        if (student == null) return NotFound();

        var todos = await todoRepository.GetTodosByUserIdAsync(student.UserId);
        return Ok(todos);
    }

    /// <summary>
    /// 添加新的待办事项
    /// </summary>
    /// <param name="todoModel">待办事项模型</param>
    /// <returns>添加后的待办事项</returns>
    [TokenActionFilter]
    [Authorize]
    [HttpPost("todos")]
    public async Task<ActionResult<TodoModel>> AddTodo(TodoModel todoModel)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();

        var student = await studentRepository.GetByIdAsync(member.UserId);
        if (student == null) return NotFound();

        todoModel.StudentId = student.UserId;
        todoModel.Id = todoModel.ToHash();
        todoModel.CreatedTime = DateTime.UtcNow;

        var result = await todoRepository.AddTodoAsync(todoModel);
        if (!result)
            return StatusCode(500, "添加待办事项失败");

        return CreatedAtAction(nameof(GetTodoById), new { id = todoModel.Id }, todoModel);
    }

    /// <summary>
    /// 删除指定ID的待办事项
    /// </summary>
    /// <param name="id">待办事项ID</param>
    /// <returns>操作结果</returns>
    [TokenActionFilter]
    [Authorize]
    [HttpDelete("todos/{id}")]
    public async Task<IActionResult> DeleteTodo(string id)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();

        var hasPermission = await todoRepository.HasPermissionAsync(id, member.UserId);
        if (!hasPermission)
            return Forbid("无权删除此待办事项");

        var result = await todoRepository.DeleteTodoAsync(id);
        if (!result)
            return NotFound("待办事项不存在或删除失败");

        return Ok();
    }

    /// <summary>
    /// 根据ID获取待办事项详情
    /// </summary>
    [HttpGet("todos/{id}")]
    public async Task<ActionResult<TodoModel>> GetTodoById(string id)
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

    /// <summary>
    /// 更新待办事项
    /// </summary>
    /// <param name="todoModel">更新后的待办事项模型</param>
    /// <returns>操作结果</returns>
    [TokenActionFilter]
    [Authorize]
    [HttpPut("todos")]
    public async Task<IActionResult> UpdateTodo(TodoModel todoModel)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();

        var hasPermission = await todoRepository.HasPermissionAsync(todoModel.Id, member.UserId);
        if (!hasPermission)
            return Forbid("无权修改此待办事项");

        var result = await todoRepository.UpdateTodoAsync(todoModel);
        if (!result)
            return NotFound("待办事项不存在或更新失败");

        return Ok();
    }

    /// <summary>
    /// 更新用户个人资料
    /// </summary>
    /// <param name="memberModel">更新后的用户资料模型</param>
    /// <returns>操作结果</returns>
    [TokenActionFilter]
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(StudentModel memberModel)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null || member.UserId != memberModel.UserId) return Forbid();

        var result = await studentRepository.UpdateAsync(memberModel);
        if (!result)
            return StatusCode(500, "更新用户资料失败");

        return NoContent();
    }
}