using iOSClub.Data;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoModel = iOSClub.Data.DataModels.TodoModel;
using MemberModel = iOSClub.Data.ShowModels.MemberModel;
using StudentModel = iOSClub.Data.DataModels.StudentModel;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(
    IDbContextFactory<iOSContext> factory,
    IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    [TokenActionFilter]
    [Authorize]
    [HttpGet("data")]
    public async Task<ActionResult<MemberModel>> GetData()
    {
        await using var context = await factory.CreateDbContextAsync();
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();
        if (member.Identity == "Founder") return member;
        var student = await context.Students.FirstOrDefaultAsync(x => x.UserId == member.UserId);
        if (student == null) return NotFound();
        var id = member.Identity;
        member = MemberModel.AutoCopy<StudentModel, MemberModel>(student);
        member.Identity = id;

        return member;
    }

    [TokenActionFilter]
    [Authorize]
    [HttpGet("todos")]
    public async Task<ActionResult<List<TodoModel>>> GetTodos()
    {
        await using var context = await factory.CreateDbContextAsync();
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();
        var student = await context.Students.FirstOrDefaultAsync(x => x.UserId == member.UserId);
        if (student == null) return NotFound();
        return await context.Todos.Where(x => x.StudentId == student.UserId).ToListAsync();
    }

    [TokenActionFilter]
    [Authorize]
    [HttpPost("todos")]
    public async Task<ActionResult<TodoModel>> AddTodo(TodoModel todoModel)
    {
        await using var context = await factory.CreateDbContextAsync();
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();
        var student = await context.Students.FirstOrDefaultAsync(x => x.UserId == member.UserId);
        if (student == null) return NotFound();
        todoModel.Student = student;
        todoModel.Id = todoModel.ToHash();
        await context.Todos.AddAsync(todoModel);
        await context.SaveChangesAsync();

        return todoModel;
    }

    [TokenActionFilter]
    [Authorize]
    [HttpDelete("todos/{id}")]
    public async Task<IActionResult> DeleteTodo(string id)
    {
        await using var context = await factory.CreateDbContextAsync();
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();
        var student = await context.Students.FirstOrDefaultAsync(x => x.UserId == member.UserId);
        if (student == null) return NotFound();

        var todo = await context.Todos.FirstOrDefaultAsync(x => x.Id == id);
        if (todo == null || todo.StudentId != member.UserId) return NotFound();

        context.Todos.Remove(todo);
        await context.SaveChangesAsync();

        return Ok();
    }

    [TokenActionFilter]
    [Authorize]
    [HttpPut("todos")]
    public async Task<IActionResult> UpdateTodo(TodoModel todoModel)
    {
        await using var context = await factory.CreateDbContextAsync();
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();
        var student = await context.Students.FirstOrDefaultAsync(x => x.UserId == member.UserId);
        if (student == null) return NotFound();

        var todo = await context.Todos.FirstOrDefaultAsync(x => x.Id == todoModel.Id);
        if (todo == null) return NotFound();

        todo.Update(todoModel);

        await context.SaveChangesAsync();
        return Ok();
    }

    [TokenActionFilter]
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(MemberModel memberModel)
    {
        await using var context = await factory.CreateDbContextAsync();
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null || member.UserId != memberModel.UserId) return Forbid();

        context.Entry(memberModel).State = EntityState.Modified;
        await context.SaveChangesAsync();

        return NoContent();
    }
}