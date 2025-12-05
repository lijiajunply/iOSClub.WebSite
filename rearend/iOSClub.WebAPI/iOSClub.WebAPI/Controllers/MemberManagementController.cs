using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iOSClub.WebAPI.Controllers;

/// <summary>
/// 成员管理控制器 - 负责成员的删除、更新等管理功能
/// </summary>
[Authorize(Roles = "Founder, President, Minister")]
[ApiController]
[Route("[controller]")]
public class MemberManagementController(
    IStudentRepository studentRepository,
    ILogger<MemberManagementController> logger) : ControllerBase
{
    /// <summary>
    /// 删除学生成员
    /// </summary>
    /// <param name="id">学生ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("delete/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(string id)
    {
        try
        {
            var result = await studentRepository.DeleteAsync(id);
            if (!result)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("删除学生失败，学生不存在，ID: {Id}", id);
                }

                return Ok(ApiResponse<object>.Fail(ErrorCode.UserNotFound, "学生不存在"));
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("删除学生成功，ID: {Id}", id);
            }

            return Ok(ApiResponse.Success("删除学生成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "删除学生时发生错误，ID: {Id}", id);
            }

            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "删除学生失败"));
        }
    }

    /// <summary>
    /// 批量更新或添加学生成员
    /// </summary>
    /// <param name="list">学生列表</param>
    /// <returns>更新后的学生列表</returns>
    [HttpPost("update-many")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateMany(List<StudentModel> list)
    {
        try
        {
            var result = await studentRepository.UpdateManyAsync(list);
            if (result)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("批量更新学生成功，更新数量: {Count}", list.Count);
                }

                return Ok(ApiResponse<bool>.Success(true, "批量更新学生成功"));
            }
            else
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("批量更新学生失败，更新数量: {Count}", list.Count);
                }

                return Ok(ApiResponse<bool>.Fail(ErrorCode.OperationFailed, "批量更新学生失败"));
            }
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "批量更新学生时发生错误，更新数量: {Count}", list.Count);
            }

            return Ok(ApiResponse<bool>.Fail(ErrorCode.InternalServerError, "批量更新学生失败"));
        }
    }

    /// <summary>
    /// 更新单个成员信息
    /// </summary>
    /// <param name="model">成员模型</param>
    /// <returns>操作结果</returns>
    [HttpPost("update")]
    public async Task<ActionResult<ApiResponse<object>>> Update([FromBody] StudentModel model)
    {
        try
        {
            var result = await studentRepository.UpdateAsync(model);
            if (!result)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("更新学生信息失败，学生不存在，ID: {Id}", model.UserId);
                }

                return Ok(ApiResponse<object>.Fail(ErrorCode.UserNotFound, "学生不存在"));
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("更新学生信息成功，ID: {Id}", model.UserId);
            }

            return Ok(ApiResponse.Success("更新学生信息成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "更新学生信息时发生错误，ID: {Id}", model.UserId);
            }

            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "更新学生信息失败"));
        }
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(ResetPasswordData data)
    {
        try
        {
            var student = await studentRepository.GetByIdAsync(data.UserId);
            if (student == null)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("重置密码失败，学生不存在，ID: {Id}", data.UserId);
                }

                return Ok(ApiResponse<object>.Fail(ErrorCode.UserNotFound, "学生不存在"));
            }

            student.PasswordHash = data.NewPassword.ToHash();
            var result = await studentRepository.UpdateAsync(student);
            if (!result)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("重置密码失败，ID: {Id}", data.UserId);
                }

                return Ok(ApiResponse<object>.Fail(ErrorCode.OperationFailed, "重置密码失败"));
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("重置密码成功，ID: {Id}", data.UserId);
            }

            return Ok(ApiResponse.Success("重置密码成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "重置密码时发生错误，ID: {Id}", data.UserId);
            }

            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "重置密码失败"));
        }
    }
}

public record ResetPasswordData(string UserId, string NewPassword);