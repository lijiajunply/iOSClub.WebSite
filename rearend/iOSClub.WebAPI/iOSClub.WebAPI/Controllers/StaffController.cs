using iOSClub.Data.DataModels;
using iOSClub.Data.ShowModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iOSClub.WebAPI.Controllers;

[Authorize(Roles = "Founder, President, Minister")]
[ApiController]
[Route("[controller]")] // 使用C#推荐的API路径格式
public class StaffController(IStaffRepository staffRepository)
    : ControllerBase
{

    /// <summary>
    /// 获取所有员工列表
    /// </summary>
    /// <returns>员工列表</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StaffModel>>> GetAllStaff()
    {
        var staffs = await staffRepository.GetAllStaffAsync();
        return Ok(staffs);
    }

    [HttpGet("members")]
    public async Task<ActionResult<IEnumerable<MemberModel>>> GetAllStaffToMembers()
    {
        var staffs = await staffRepository.GetAllStaffToMembers();
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
        var result = await staffRepository.CreateStaffAsync(staff);
        if (!result)
            return BadRequest("创建成员失败");

        return CreatedAtAction(nameof(GetStaff), new { userId = staff.UserId }, staff);
    }

    /// <summary>
    /// 更新员工信息
    /// </summary>
    /// <param name="staff">更新后的员工信息</param>
    /// <returns>更新结果</returns>
    [HttpPost("Update")]
    public async Task<ActionResult> UpdateStaff([FromBody] StaffModel staff)
    {
        // 获取目标成员信息
        var targetStaff = await staffRepository.GetStaffByIdAsync(staff.UserId);
        if (targetStaff == null)
            return NotFound();

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
        // 获取目标成员信息
        var targetStaff = await staffRepository.GetStaffByIdAsync(userId);
        if (targetStaff == null)
            return NotFound();

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
       var result = await staffRepository.ChangeStaffDepartmentAsync(userId, departmentName);
        if (!result)
            return BadRequest("修改部门失败");

        return Ok();
    }
}