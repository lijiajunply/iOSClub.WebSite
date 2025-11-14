using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iOSClub.WebAPI.Controllers;

/// <summary>
/// 成员管理控制器 - 负责成员的删除、更新等管理功能
/// </summary>
[Authorize(Roles = "Founder, President, Minister")]
[TokenActionFilter]
[ApiController]
[Route("[controller]")]
public class MemberManagementController(IStudentRepository studentRepository) : ControllerBase
{
    /// <summary>
    /// 删除学生成员
    /// </summary>
    /// <param name="id">学生ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await studentRepository.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// 批量更新或添加学生成员
    /// </summary>
    /// <param name="list">学生列表</param>
    /// <returns>更新后的学生列表</returns>
    [HttpPost("update-many")]
    public async Task<ActionResult<bool>> UpdateMany(List<StudentModel> list)
    {
        var result = await studentRepository.UpdateManyAsync(list);
        return result ? Ok() : NotFound();
    }

    /// <summary>
    /// 更新单个成员信息
    /// </summary>
    /// <param name="model">成员模型</param>
    /// <returns>操作结果</returns>
    [HttpPost("update")]
    public async Task<ActionResult> Update([FromBody] StudentModel model)
    {
        var result = await studentRepository.UpdateAsync(model);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordData data)
    {
        var student = await studentRepository.GetByIdAsync(data.UserId);
        if (student == null)
            return NotFound();

        student.PasswordHash = data.NewPassword.ToHash();
        var result = await studentRepository.UpdateAsync(student);
        if (!result)
            return NotFound();

        return Ok();
    }
}

public record ResetPasswordData(string UserId, string NewPassword);