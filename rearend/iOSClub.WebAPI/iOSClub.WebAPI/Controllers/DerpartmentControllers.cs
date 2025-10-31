using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.WebAPI.Controllers;

[Authorize]
[TokenActionFilter]
[ApiController]
[Route("[controller]")]  // 使用C#推荐的API路径格式
public class DepartmentController(
    IDbContextFactory<iOSContext> factory,
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

            await using var context = await factory.CreateDbContextAsync();

            var user = await context.Staffs
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.UserId == userJwt.UserId);

            if (user == null)
                return NotFound("用户不存在");

            // 检查用户权限
            if (!IsAuthorizedUser(user.Identity))
                return Forbid("权限不足");

            // 如果没有指定部门名称，返回用户所在部门
            if (string.IsNullOrEmpty(name))
            {
                if (user.Department == null)
                    return NotFound("用户未分配部门");

                return Ok(user.Department);
            }

            // 管理员可以查看任何部门
            if (IsAdminUser(user.Identity))
            {
                var department = await context.Departments
                    .Include(d => d.Staffs)
                    .Include(d => d.Projects)
                    .FirstOrDefaultAsync(x => x.Name == name);
                
                if (department == null)
                    return NotFound("部门不存在");
                
                return Ok(department);
            }

            // 普通用户只能查看自己部门
            if (user.Department == null || user.Department.Name != name)
                return Forbid("只能查看自己部门的信息");

            return Ok(user.Department);
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
            await using var context = await factory.CreateDbContextAsync();
            
            var departments = await context.Departments
                .Include(d => d.Staffs)
                .Include(d => d.Projects)
                .ToListAsync();
            
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
    [HttpPut("Update")]
    [Authorize(Roles = "Founder,President,Minister")]
    public async Task<IActionResult> UpdateDepartment([FromBody] DepartmentModel model)
    {
        try
        {
            await using var context = await factory.CreateDbContextAsync();

            var department = await context.Departments
                .FirstOrDefaultAsync(x => x.Name == model.Name);
            
            if (department == null)
                return NotFound("部门不存在");

            // 更新可修改的字段
            department.Description = model.Description;
            department.Key = model.Key; // 如果需要更新Key

            await context.SaveChangesAsync();
            return Ok("部门信息更新成功");
        }
        catch (DbUpdateException ex)
        {
            return BadRequest($"更新失败: {ex.InnerException?.Message ?? ex.Message}");
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
            await using var context = await factory.CreateDbContextAsync();

            // 检查部门是否已存在
            if (await context.Departments.AnyAsync(d => d.Name == model.Name))
                return BadRequest("部门已存在");

            await context.Departments.AddAsync(model);
            await context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetDepartment), new { name = model.Name }, model);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"创建失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除部门
    /// </summary>
    [HttpDelete("Delete/{name}")]
    [Authorize(Roles = "Founder,President")]
    public async Task<IActionResult> DeleteDepartment(string name)
    {
        try
        {
            await using var context = await factory.CreateDbContextAsync();

            var department = await context.Departments
                .Include(d => d.Staffs)
                .Include(d => d.Projects)
                .FirstOrDefaultAsync(d => d.Name == name);
            
            if (department == null)
                return NotFound("部门不存在");

            // 检查部门是否有成员或项目
            if (department.Staffs.Any())
                return BadRequest("无法删除包含成员的部门");
            
            if (department.Projects.Any())
                return BadRequest("无法删除包含项目的部门");

            context.Departments.Remove(department);
            await context.SaveChangesAsync();
            
            return Ok("部门删除成功");
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
    public async Task<IActionResult> AddStaffToDepartment(string departmentName, string userId)
    {
        try
        {
            await using var context = await factory.CreateDbContextAsync();

            var department = await context.Departments
                .FirstOrDefaultAsync(d => d.Name == departmentName);
            
            if (department == null)
                return NotFound("部门不存在");

            var staff = await context.Staffs
                .FirstOrDefaultAsync(s => s.UserId == userId);
            
            if (staff == null)
                return NotFound("成员不存在");

            staff.Department = department;
            await context.SaveChangesAsync();
            
            return Ok("成员添加成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"添加失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 从部门移除成员
    /// </summary>
    [HttpDelete("RemoveStaff")]
    [Authorize(Roles = "Founder,President,Minister")]
    public async Task<IActionResult> RemoveStaffFromDepartment(string userId)
    {
        try
        {
            await using var context = await factory.CreateDbContextAsync();

            var staff = await context.Staffs
                .FirstOrDefaultAsync(s => s.UserId == userId);
            
            if (staff == null)
                return NotFound("成员不存在");

            staff.Department = null;
            await context.SaveChangesAsync();
            
            return Ok("成员移除成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"移除失败: {ex.Message}");
        }
    }

    // 辅助方法：检查用户是否有权限
    private bool IsAuthorizedUser(string? identity)
    {
        return identity is "Founder" or "President" or "Minister" or "Department";
    }

    // 辅助方法：检查用户是否是管理员
    private bool IsAdminUser(string? identity)
    {
        return identity is "Founder" or "President" or "Minister";
    }
}