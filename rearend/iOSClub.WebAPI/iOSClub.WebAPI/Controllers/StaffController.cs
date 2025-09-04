using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iOSClub.WebAPI.Controllers;

[Authorize(Roles = "Founder, President, Minister")]
[TokenActionFilter]
[Route("[controller]")]
[ApiController]
public class StaffController(IStaffRepository staffRepository, IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    // 获取当前用户信息（用于权限验证）
    private StaffModel? GetCurrentUser()
    {
        return httpContextAccessor.HttpContext?.User.GetStaff();
    }

    // 检查是否有管理权限（President 或 Minister）
    private bool HasManagementPermission(StaffModel? staff)
    {
        return staff is { Identity: "President" or "Minister" or "Founder" };
    }

    // 检查是否可以修改目标成员（不能修改比自己权限高的成员）
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

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<StaffModel>>> GetAllStaff()
    {
        var currentStaff = GetCurrentUser();
        if (currentStaff == null || !HasManagementPermission(currentStaff))
            return Forbid();

        var staffs = await staffRepository.GetAllStaffAsync();
        return Ok(staffs);
    }

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

    [HttpGet("by-identity/{identity}")]
    public async Task<ActionResult<IEnumerable<StaffModel>>> GetStaffsByIdentity(string identity)
    {
        var currentStaff = GetCurrentUser();
        if (currentStaff == null || !HasManagementPermission(currentStaff))
            return Forbid();

        var staffs = await staffRepository.GetStaffsByIdentitiesAsync(identity);
        return Ok(staffs);
    }

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