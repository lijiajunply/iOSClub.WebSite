using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.WebAPI.Controllers;

[Authorize]
[TokenActionFilter]
[ApiController]
[Route("[controller]")]  // 使用C#推荐的API路径格式
public class ResourceController(
    IDbContextFactory<iOSContext> factory,
    IHttpContextAccessor httpContextAccessor,
    ResourceRepository resourceRepository)
    : ControllerBase
{
    /// <summary>
    /// 获取所有资源（需要社团成员身份）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ResourceModel>>> GetAllResources()
    {
        try
        {
            var userJwt = httpContextAccessor.HttpContext?.User.GetUser();
            if (userJwt == null)
                return Unauthorized("用户未认证");

            await using var context = await factory.CreateDbContextAsync();
            var user = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == userJwt.UserId);

            // 检查用户身份：必须是社团成员及以上
            if (user == null || !IsClubMember(user.Identity))
                return Forbid("权限不足，需要社团成员身份");

            var resources = await resourceRepository.GetAllResourcesAsync();
            return Ok(resources);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取资源（需要社团成员身份）
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceModel>> GetResourceById(string id)
    {
        try
        {
            var userJwt = httpContextAccessor.HttpContext?.User.GetUser();
            if (userJwt == null)
                return Unauthorized("用户未认证");

            await using var context = await factory.CreateDbContextAsync();
            var user = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == userJwt.UserId);

            // 检查用户身份：必须是社团成员及以上
            if (user == null || !IsClubMember(user.Identity))
                return Forbid("权限不足，需要社团成员身份");

            var resource = await resourceRepository.GetResourceByIdAsync(id);
            if (resource == null)
                return NotFound("资源不存在");

            return Ok(resource);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据标签筛选资源（需要社团成员身份）
    /// </summary>
    [HttpGet("tag/{tag}")]
    public async Task<ActionResult<List<ResourceModel>>> GetResourcesByTag(string tag)
    {
        try
        {
            var userJwt = httpContextAccessor.HttpContext?.User.GetUser();
            if (userJwt == null)
                return Unauthorized("用户未认证");

            await using var context = await factory.CreateDbContextAsync();
            var user = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == userJwt.UserId);

            // 检查用户身份：必须是社团成员及以上
            if (user == null || !IsClubMember(user.Identity))
                return Forbid("权限不足，需要社团成员身份");

            var resources = await resourceRepository.GetResourcesByTagAsync(tag);
            return Ok(resources);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 搜索资源（需要社团成员身份）
    /// </summary>
    [HttpGet("search/{name}")]
    public async Task<ActionResult<List<ResourceModel>>> SearchResources(string name)
    {
        try
        {
            var userJwt = httpContextAccessor.HttpContext?.User.GetUser();
            if (userJwt == null)
                return Unauthorized("用户未认证");

            await using var context = await factory.CreateDbContextAsync();
            var user = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == userJwt.UserId);

            // 检查用户身份：必须是社团成员及以上
            if (user == null || !IsClubMember(user.Identity))
                return Forbid("权限不足，需要社团成员身份");

            var resources = await resourceRepository.SearchResourcesByNameAsync(name);
            return Ok(resources);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 添加资源（需要社长、团支书或部长身份）
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Founder,President,Minister")]
    public async Task<ActionResult<ResourceModel>> AddResource([FromBody] ResourceModel resource)
    {
        try
        {
            var userJwt = httpContextAccessor.HttpContext?.User.GetUser();
            if (userJwt == null)
                return Unauthorized("用户未认证");

            // 双重验证：除了角色授权外，再检查用户身份
            await using var context = await factory.CreateDbContextAsync();
            var user = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == userJwt.UserId);
            
            if (user == null || !IsAdmin(user.Identity))
                return Forbid("权限不足，需要管理员身份");

            var result = await resourceRepository.AddResourceAsync(resource);
            if (!result)
                return BadRequest("添加资源失败");

            return CreatedAtAction(nameof(GetResourceById), new { id = resource.Id }, resource);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新资源（需要社长、团支书或部长身份）
    /// </summary>
    [HttpPut]
    [Authorize(Roles = "Founder,President,Minister")]
    public async Task<IActionResult> UpdateResource([FromBody] ResourceModel resource)
    {
        try
        {
            var userJwt = httpContextAccessor.HttpContext?.User.GetUser();
            if (userJwt == null)
                return Unauthorized("用户未认证");

            // 双重验证
            await using var context = await factory.CreateDbContextAsync();
            var user = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == userJwt.UserId);
            
            if (user == null || !IsAdmin(user.Identity))
                return Forbid("权限不足，需要管理员身份");

            // 检查资源是否存在
            var existingResource = await resourceRepository.GetResourceByIdAsync(resource.Id);
            if (existingResource == null)
                return NotFound("资源不存在");

            var result = await resourceRepository.UpdateResourceAsync(resource);
            if (!result)
                return BadRequest("更新资源失败");

            return Ok("资源更新成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除资源（需要社长、团支书或部长身份）
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Founder,President,Minister")]
    public async Task<IActionResult> DeleteResource(string id)
    {
        try
        {
            var userJwt = httpContextAccessor.HttpContext?.User.GetUser();
            if (userJwt == null)
                return Unauthorized("用户未认证");

            // 双重验证
            await using var context = await factory.CreateDbContextAsync();
            var user = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == userJwt.UserId);
            
            if (user == null || !IsAdmin(user.Identity))
                return Forbid("权限不足，需要管理员身份");

            // 检查资源是否存在
            var existingResource = await resourceRepository.GetResourceByIdAsync(id);
            if (existingResource == null)
                return NotFound("资源不存在");

            var result = await resourceRepository.DeleteResourceAsync(id);
            if (!result)
                return BadRequest("删除资源失败");

            return Ok("资源删除成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取所有标签（需要社团成员身份）
    /// </summary>
    [HttpGet("tags")]
    public async Task<ActionResult<List<string>>> GetAllTags()
    {
        try
        {
            var userJwt = httpContextAccessor.HttpContext?.User.GetUser();
            if (userJwt == null)
                return Unauthorized("用户未认证");

            await using var context = await factory.CreateDbContextAsync();
            var user = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == userJwt.UserId);

            // 检查用户身份：必须是社团成员及以上
            if (user == null || !IsClubMember(user.Identity))
                return Forbid("权限不足，需要社团成员身份");

            var tags = await resourceRepository.GetAllTagsAsync();
            return Ok(tags);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取资源统计（需要社团成员身份）
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<object>> GetResourceStatistics()
    {
        try
        {
            var userJwt = httpContextAccessor.HttpContext?.User.GetUser();
            if (userJwt == null)
                return Unauthorized("用户未认证");

            await using var context = await factory.CreateDbContextAsync();
            var user = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == userJwt.UserId);

            // 检查用户身份：必须是社团成员及以上
            if (user == null || !IsClubMember(user.Identity))
                return Forbid("权限不足，需要社团成员身份");

            var count = await resourceRepository.GetResourceCountAsync();
            var tags = await resourceRepository.GetAllTagsAsync();

            return Ok(new
            {
                TotalCount = count,
                TagCount = tags.Count,
                Tags = tags
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 检查用户是否是社团成员及以上身份
    /// </summary>
    private static bool IsClubMember(string? identity)
    {
        return identity is "Founder" or "President" or "Minister" or "Department";
    }

    /// <summary>
    /// 检查用户是否是管理员身份
    /// </summary>
    private static bool IsAdmin(string? identity)
    {
        return identity is "Founder" or "President" or "Minister";
    }
}