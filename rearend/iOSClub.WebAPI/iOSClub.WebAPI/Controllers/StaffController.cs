using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iOSClub.WebAPI.Controllers;

[Authorize(Roles = "Founder, President, Minister")]
[TokenActionFilter]
[ApiController]
[Route("[controller]")]  // 使用C#推荐的API路径格式
public class StaffController(IStaffRepository staffRepository, IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    /// <summary>
    /// 获取当前用户信息（用于权限验证）
    /// </summary>
    /// <returns>当前用户的StaffModel对象，如果无法获取则返回null</returns>
    private StaffModel? GetCurrentUser()
    {
        return httpContextAccessor.HttpContext?.User.GetStaff();
    }

    /// <summary>
    /// 检查是否有管理权限（创始人、社长或部长）
    /// </summary>
    /// <param name="staff">要检查的用户</param>
    /// <returns>如果有管理权限则返回true，否则返回false</returns>
    private bool HasManagementPermission(StaffModel? staff)
    {
        return staff is { Identity: "President" or "Minister" or "Founder" };
    }

    /// <summary>
    /// 检查是否可以修改目标成员（不能修改比自己权限高的成员）
    /// </summary>
    /// <param name="currentStaff">当前用户</param>
    /// <param name="targetStaff">目标成员</param>
    /// <returns>如果可以修改则返回true，否则返回false</returns>
    private bool CanModifyTarget(StaffModel? currentStaff, StaffModel targetStaff)
    {
        if (currentStaff == null)
            return false;

        // Founder 可以修改任何人
        if (currentStaff.Identity == "Founder")
            return true;

        // President 可以修改 Minister、Department、Member
        if (currentStaff.Identity == "President")
            return targetStaff.Identity is "Minister" or "Department" or "Member";

        // Minister 只能修改 Department 和 Member
        if (currentStaff.Identity == "Minister")
            return targetStaff.Identity is "Department" or "Member";

        return false;
    }

    /// <summary>
    /// 获取所有员工列表
    /// </summary>
    /// <returns>员工列表</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StaffModel>>> GetAllStaff()
    {
        var currentStaff = GetCurrentUser();
        if (currentStaff == null || !HasManagementPermission(currentStaff))
            return Forbid();

        var staffs = await staffRepository.GetAllStaffAsync();
        return Ok(staffs);
    }

    /// <summary>
    /// 根据用户ID获取员工信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>员工信息</returns>
    [HttpGet("{userId}")]
    public async Task<ActionResult<StaffModel>> GetStaff(string userId)
    {
        var currentStaff = GetCurrentUser();
        if (currentStaff == null || !HasManagementPermission(currentStaff))
            return Forbid();

        var staff = await staffRepository.GetStaffByIdAsync(userId);
        if (staff == null)
            return NotFound();

        return Ok(staff);
    }
    
    /// <summary>
    /// 创建新员工
    /// </summary>
    /// <param name="staff">员工信息模型</param>
    /// <returns>创建结果</returns>
    [HttpPost("Create")]
    public async Task<ActionResult> CreateStaff([FromBody] StaffModel staff)
    {
        var currentStaff = GetCurrentUser();
        if (currentStaff == null || !HasManagementPermission(currentStaff))
            return Forbid();

        // 检查是否可以创建该权限级别的成员
        if (!CanModifyTarget(currentStaff, staff))
            return BadRequest("没有权限创建该级别的成员");

        var result = await staffRepository.CreateStaffAsync(staff);
        if (!result)
            return BadRequest("创建成员失败");

        return CreatedAtAction(nameof(GetStaff), new { userId = staff.UserId }, staff);
    }

    /// <summary>
    /// 更新员工信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="staff">更新后的员工信息</param>
    /// <returns>更新结果</returns>
    [HttpPost("Update/{userId}")]
    public async Task<ActionResult> UpdateStaff(string userId, [FromBody] StaffModel staff)
    {
        var currentStaff = GetCurrentUser();
        if (currentStaff == null || !HasManagementPermission(currentStaff))
            return Forbid();

        if (userId != staff.UserId)
            return BadRequest("用户ID不匹配");

        // 获取目标成员信息
        var targetStaff = await staffRepository.GetStaffByIdAsync(userId);
        if (targetStaff == null)
            return NotFound();

        // 检查是否可以修改目标成员
        if (!CanModifyTarget(currentStaff, targetStaff))
            return BadRequest("没有权限修改该成员");

        var result = await staffRepository.UpdateStaffAsync(staff);
        if (!result)
            return BadRequest("更新成员失败");

        return Ok();
    }
    
    /// <summary>
    /// 删除员工
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>删除结果</returns>
    [HttpGet("Delete/{userId}")]
    public async Task<ActionResult> DeleteStaff(string userId)
    {
        var currentStaff = GetCurrentUser();
        if (currentStaff == null || !HasManagementPermission(currentStaff))
            return Forbid();

        // 获取目标成员信息
        var targetStaff = await staffRepository.GetStaffByIdAsync(userId);
        if (targetStaff == null)
            return NotFound();

        // 检查是否可以删除目标成员
        if (!CanModifyTarget(currentStaff, targetStaff))
            return BadRequest("没有权限删除该成员");

        var result = await staffRepository.DeleteStaffAsync(userId);
        if (!result)
            return BadRequest("删除成员失败");

        return Ok();
    }

    /// <summary>
    /// 根据身份获取员工列表
    /// </summary>
    /// <param name="identity">员工身份</param>
    /// <returns>符合条件的员工列表</returns>
    [HttpGet("by-identity/{identity}")]
    public async Task<ActionResult<IEnumerable<StaffModel>>> GetStaffsByIdentity(string identity)
    {
        var currentStaff = GetCurrentUser();
        if (currentStaff == null || !HasManagementPermission(currentStaff))
            return Forbid();

        var staffs = await staffRepository.GetStaffsByIdentitiesAsync(identity);
        return Ok(staffs);
    }

    /// <summary>
    /// 修改员工所属部门
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="departmentName">部门名称，为null时表示移除部门</param>
    /// <returns>修改结果</returns>
    [HttpPost("change-department/{userId}")]
    public async Task<ActionResult> ChangeStaffDepartment(string userId, [FromQuery] string? departmentName)
    {
        var currentStaff = GetCurrentUser();
        if (currentStaff == null || !HasManagementPermission(currentStaff))
            return Forbid();

        var result = await staffRepository.ChangeStaffDepartmentAsync(userId, departmentName);
        if (!result)
            return BadRequest("修改部门失败");

        return Ok();
    }
}