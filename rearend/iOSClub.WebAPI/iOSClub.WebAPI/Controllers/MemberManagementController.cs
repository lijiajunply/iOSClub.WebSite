using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.Data.ShowModels;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.WebAPI.Controllers;

/// <summary>
/// 成员管理控制器 - 负责成员的删除、更新等管理功能
/// </summary>
[Authorize(Roles = "Founder, President, Minister")]
[TokenActionFilter]
[ApiController]
[Route("[controller]")]
public class MemberManagementController(IDbContextFactory<iOSContext> factory) : ControllerBase
{
    /// <summary>
    /// 删除学生成员
    /// </summary>
    /// <param name="id">学生ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await using var context = await factory.CreateDbContextAsync();
        if (context.Students == null!)
            return NotFound();

        var memberModel = await context.Students.FindAsync(id);

        if (memberModel == null)
            return NotFound();

        context.Students.Remove(memberModel);
        await context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// 批量更新或添加学生成员
    /// </summary>
    /// <param name="list">学生列表</param>
    /// <returns>更新后的学生列表</returns>
    [HttpPost("update-many")]
    public async Task<ActionResult<List<StudentModel>>> UpdateMany(List<StudentModel> list)
    {
        await using var context = await factory.CreateDbContextAsync();

        // 获取所有现有的学生ID
        var existingStudentIds = await context.Students
            .Select(s => s.UserId)
            .ToHashSetAsync();

        // 过滤出只需要添加的学生
        var newStudents = list
            .Where(model => !existingStudentIds.Contains(model.UserId))
            .Select(model => model.Standardization())
            .ToList();

        // 批量添加新学生
        if (newStudents.Count > 0)
        {
            await context.Students.AddRangeAsync(newStudents);
            await context.SaveChangesAsync();
        }

        return await context.Students.ToListAsync();
    }

    /// <summary>
    /// 更新单个成员信息
    /// </summary>
    /// <param name="model">成员模型</param>
    /// <returns>操作结果</returns>
    [HttpPost("update")]
    public async Task<ActionResult> Update([FromBody] MemberModel model)
    {
        await using var context = await factory.CreateDbContextAsync();
        if (context.Students == null!)
        {
            return NotFound();
        }

        context.Entry(model).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await context.Students.AnyAsync(x => x.Equals(model));
            if (!exists)
                return NotFound();
            throw;
        }

        return NoContent();
    }
}