using iOSClub.Data.DataModels;
using iOSClub.Data.ShowModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iOSClub.WebAPI.Controllers;

[Authorize(Roles = "Founder, President, Minister")]
[ApiController]
[Route("[controller]")] // 使用C#推荐的API路径格式
public class StaffController(IStaffRepository staffRepository, ILogger<StaffController> logger)
    : ControllerBase
{
    /// <summary>
    /// 获取所有员工列表
    /// </summary>
    /// <returns>员工列表</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<StaffModel>>>> GetAllStaff()
    {
        try
        {
            var staffs = await staffRepository.GetAllStaffAsync();
            var staffModels = staffs as StaffModel[] ?? staffs.ToArray();
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("获取所有员工列表成功，员工数量: {Count}", staffModels.Length);
            }

            return Ok(ApiResponse<IEnumerable<StaffModel>>.Success(staffModels, "获取所有员工列表成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "获取所有员工列表失败");
            }

            return Ok(ApiResponse<IEnumerable<StaffModel>>.Fail(ErrorCode.InternalServerError, "获取所有员工列表失败"));
        }
    }

    [HttpGet("members")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MemberModel>>>> GetAllStaffToMembers()
    {
        try
        {
            var staffs = await staffRepository.GetAllStaffToMembers();
            var memberModels = staffs as MemberModel[] ?? staffs.ToArray();
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("获取所有成员列表成功，成员数量: {Count}", memberModels.Length);
            }

            return Ok(ApiResponse<IEnumerable<MemberModel>>.Success(memberModels, "获取所有成员列表成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "获取所有成员列表失败");
            }

            return Ok(ApiResponse<IEnumerable<MemberModel>>.Fail(ErrorCode.InternalServerError, "获取所有成员列表失败"));
        }
    }

    /// <summary>
    /// 根据用户ID获取员工信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>员工信息</returns>
    [HttpGet("{userId}")]
    public async Task<ActionResult<ApiResponse<StaffModel>>> GetStaff(string userId)
    {
        try
        {
            var staff = await staffRepository.GetStaffByIdAsync(userId);
            if (staff == null)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("获取员工信息失败，员工不存在，ID: {UserId}", userId);
                }

                return Ok(ApiResponse<StaffModel>.Fail(ErrorCode.UserNotFound, "员工不存在"));
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("获取员工信息成功，ID: {UserId}", userId);
            }

            return Ok(ApiResponse<StaffModel>.Success(staff, "获取员工信息成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "获取员工信息失败，ID: {UserId}", userId);
            }

            return Ok(ApiResponse<StaffModel>.Fail(ErrorCode.InternalServerError, "获取员工信息失败"));
        }
    }

    /// <summary>
    /// 创建新员工
    /// </summary>
    /// <param name="staff">员工信息模型</param>
    /// <returns>创建结果</returns>
    [HttpPost("Create")]
    public async Task<ActionResult<ApiResponse<StaffModel>>> CreateStaff([FromBody] StaffModel staff)
    {
        try
        {
            var result = await staffRepository.CreateStaffAsync(staff);
            if (!result)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("创建成员失败，ID: {UserId}, 名称: {Name}", staff.UserId, staff.Name);
                }

                return Ok(ApiResponse<StaffModel>.Fail(ErrorCode.OperationFailed, "创建成员失败"));
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("创建成员成功，ID: {UserId}, 名称: {Name}", staff.UserId, staff.Name);
            }

            return CreatedAtAction(nameof(GetStaff), new { userId = staff.UserId },
                ApiResponse<StaffModel>.Success(staff, "创建成员成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "创建成员失败，ID: {UserId}, 名称: {Name}", staff.UserId, staff.Name);
            }

            return Ok(ApiResponse<StaffModel>.Fail(ErrorCode.InternalServerError, "创建成员失败"));
        }
    }

    /// <summary>
    /// 更新员工信息
    /// </summary>
    /// <param name="staff">更新后的员工信息</param>
    /// <returns>更新结果</returns>
    [HttpPost("Update")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStaff([FromBody] StaffModel staff)
    {
        try
        {
            // 获取目标成员信息
            var targetStaff = await staffRepository.GetStaffByIdAsync(staff.UserId);
            if (targetStaff == null)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("更新成员失败，员工不存在，ID: {UserId}", staff.UserId);
                }

                return Ok(ApiResponse<object>.Fail(ErrorCode.UserNotFound, "员工不存在"));
            }

            var result = await staffRepository.UpdateStaffAsync(staff);
            if (!result)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("更新成员失败，ID: {UserId}, 名称: {Name}", staff.UserId, staff.Name);
                }

                return Ok(ApiResponse<object>.Fail(ErrorCode.OperationFailed, "更新成员失败"));
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("更新成员成功，ID: {UserId}, 名称: {Name}", staff.UserId, staff.Name);
            }

            return Ok(ApiResponse.Success("更新成员成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "更新成员失败，ID: {UserId}, 名称: {Name}", staff.UserId, staff.Name);
            }

            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "更新成员失败"));
        }
    }

    /// <summary>
    /// 删除员工
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>删除结果</returns>
    [HttpGet("Delete/{userId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteStaff(string userId)
    {
        try
        {
            // 获取目标成员信息
            var targetStaff = await staffRepository.GetStaffByIdAsync(userId);
            if (targetStaff == null)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("删除成员失败，员工不存在，ID: {UserId}", userId);
                }

                return Ok(ApiResponse<object>.Fail(ErrorCode.UserNotFound, "员工不存在"));
            }

            var result = await staffRepository.DeleteStaffAsync(userId);
            if (!result)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("删除成员失败，ID: {UserId}", userId);
                }

                return Ok(ApiResponse<object>.Fail(ErrorCode.OperationFailed, "删除成员失败"));
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("删除成员成功，ID: {UserId}", userId);
            }

            return Ok(ApiResponse.Success("删除成员成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "删除成员失败，ID: {UserId}", userId);
            }

            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "删除成员失败"));
        }
    }

    /// <summary>
    /// 根据身份获取员工列表
    /// </summary>
    /// <param name="identity">员工身份</param>
    /// <returns>符合条件的员工列表</returns>
    [HttpGet("by-identity/{identity}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<StaffModel>>>> GetStaffsByIdentity(string identity)
    {
        try
        {
            var staffs = await staffRepository.GetStaffsByIdentitiesAsync(identity);
            var staffModels = staffs as StaffModel[] ?? staffs.ToArray();
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("根据身份获取员工列表成功，身份: {Identity}, 员工数量: {Count}", identity, staffModels.Length);
            }

            return Ok(ApiResponse<IEnumerable<StaffModel>>.Success(staffModels, "根据身份获取员工列表成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "根据身份获取员工列表失败，身份: {Identity}", identity);
            }

            return Ok(ApiResponse<IEnumerable<StaffModel>>.Fail(ErrorCode.InternalServerError, "根据身份获取员工列表失败"));
        }
    }

    /// <summary>
    /// 修改员工所属部门
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="departmentName">部门名称，为null时表示移除部门</param>
    /// <returns>修改结果</returns>
    [HttpPost("change-department/{userId}")]
    public async Task<ActionResult<ApiResponse<object>>> ChangeStaffDepartment(string userId,
        [FromQuery] string? departmentName)
    {
        try
        {
            var result = await staffRepository.ChangeStaffDepartmentAsync(userId, departmentName);
            if (!result)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("修改部门失败，ID: {UserId}, 部门名称: {DepartmentName}", userId, departmentName);
                }

                return Ok(ApiResponse<object>.Fail(ErrorCode.OperationFailed, "修改部门失败"));
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("修改部门成功，ID: {UserId}, 部门名称: {DepartmentName}", userId, departmentName);
            }

            return Ok(ApiResponse.Success("修改部门成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "修改部门失败，ID: {UserId}, 部门名称: {DepartmentName}", userId, departmentName);
            }

            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "修改部门失败"));
        }
    }
}