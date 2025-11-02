using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iOSClub.WebAPI.Controllers;

[Authorize]
[TokenActionFilter]
[ApiController]
[Route("[controller]")] // 使用C#推荐的API路径格式
public class DepartmentController(
    IDepartmentRepository departmentRepository,
    IStaffRepository staffRepository,
    IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    /// <summary>
    /// 获取部门信息
    /// </summary>
    [HttpGet("{name?}")]
    public async Task<ActionResult<DepartmentModel>> GetDepartment(string? name)
    {
        try
        {
            var userJwt = httpContextAccessor.HttpContext?.User.GetUser();
            if (userJwt == null)
                return Unauthorized("用户未认证");

            // 检查用户权限
            if (!IsAuthorizedUser(userJwt.Identity))
                return Forbid("权限不足");

            var staff = await staffRepository.GetStaffByIdAsync(userJwt.UserId);
            if (staff == null) return Unauthorized("用户未找到或权限不足");
            // 如果没有指定部门名称，返回用户所在部门
            if (string.IsNullOrEmpty(name))
            {
                var userDepartment = await departmentRepository.GetDepartmentByNameAsync(staff.Department?.Name ?? "");
                if (userDepartment == null)
                    return NotFound("用户未分配部门");

                // 普通用户只能查看自己部门
                if (!IsAdminUser(userJwt.Identity) && userDepartment.Name != staff.Department?.Name)
                    return Forbid("只能查看自己部门的信息");

                return Ok(userDepartment);
            }

            // 管理员可以查看任何部门
            if (IsAdminUser(userJwt.Identity))
            {
                var department = await departmentRepository.GetDepartmentByNameAsync(name);

                if (department == null)
                    return NotFound("部门不存在");

                return Ok(department);
            }

            // 普通用户只能查看自己部门
            if (staff.Department == null || staff.Department.Name != name)
                return Forbid("只能查看自己部门的信息");

            var userDept = await departmentRepository.GetDepartmentByNameAsync(name);
            return Ok(userDept);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取所有部门（仅管理员）
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Founder,President,Minister")]
    public async Task<ActionResult<List<DepartmentModel>>> GetAllDepartments()
    {
        try
        {
            var departments = await departmentRepository.GetAllDepartmentsAsync();
            return Ok(departments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新部门信息
    /// </summary>
    [HttpPost("Update")]
    [Authorize(Roles = "Founder,President,Minister")]
    public async Task<IActionResult> UpdateDepartment([FromBody] DepartmentModel model)
    {
        try
        {
            var department = await departmentRepository.GetDepartmentByNameAsync(model.Name);

            if (department == null)
                return NotFound("部门不存在");

            // 更新可修改的字段
            department.Description = model.Description;
            department.Key = model.Key; // 如果需要更新Key

            var result = await departmentRepository.UpdateDepartmentAsync(department);
            if (!result)
                return BadRequest("更新失败");

            return Ok("部门信息更新成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建新部门
    /// </summary>
    [HttpPost("Create")]
    [Authorize(Roles = "Founder,President")]
    public async Task<IActionResult> CreateDepartment([FromBody] DepartmentModel model)
    {
        try
        {
            // 检查部门是否已存在
            var existingDept = await departmentRepository.GetDepartmentByNameAsync(model.Name);
            if (existingDept != null)
                return BadRequest("部门已存在");

            var result = await departmentRepository.AddDepartmentAsync(model);
            return !result
                ? StatusCode(500, "创建失败")
                : CreatedAtAction(nameof(GetDepartment), new { name = model.Name }, model);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"创建失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除部门
    /// </summary>
    [HttpGet("Delete/{name}")]
    [Authorize(Roles = "Founder,President")]
    public async Task<IActionResult> DeleteDepartment(string name)
    {
        try
        {
            var department = await departmentRepository.GetDepartmentByNameAsync(name);

            if (department == null)
                return NotFound("部门不存在");

            // 检查部门是否有成员或项目
            if (department.Staffs.Count != 0)
                return BadRequest("无法删除包含成员的部门");

            if (department.Projects.Count != 0)
                return BadRequest("无法删除包含项目的部门");

            var result = await departmentRepository.DeleteDepartmentAsync(name);
            return !result ? StatusCode(500, "删除失败") : Ok("部门删除成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"删除失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 添加成员到部门
    /// </summary>
    [HttpPost("AddStaff")]
    [Authorize(Roles = "Founder,President,Minister")]
    public IActionResult AddStaffToDepartment()
    {
        // 这个操作需要在StaffRepository中实现，因为涉及Staff实体的更新
        return StatusCode(501, "此功能需要在StaffController中实现");
    }

    /// <summary>
    /// 从部门移除成员
    /// </summary>
    [HttpGet("RemoveStaff")]
    [Authorize(Roles = "Founder,President,Minister")]
    public IActionResult RemoveStaffFromDepartment()
    {
        // 这个操作需要在StaffRepository中实现，因为涉及Staff实体的更新
        return StatusCode(501, "此功能需要在StaffController中实现");
    }

    /// <summary>
    /// 检查用户是否有权限访问部门信息
    /// </summary>
    /// <param name="identity">用户身份</param>
    /// <returns>如果用户有权限则返回true，否则返回false</returns>
    private static bool IsAuthorizedUser(string? identity)
    {
        return identity is "Founder" or "President" or "Minister" or "Department";
    }

    /// <summary>
    /// 检查用户是否是管理员（创始人、社长或部长）
    /// </summary>
    /// <param name="identity">用户身份</param>
    /// <returns>如果用户是管理员则返回true，否则返回false</returns>
    private static bool IsAdminUser(string? identity)
    {
        return identity is "Founder" or "President" or "Minister";
    }
}