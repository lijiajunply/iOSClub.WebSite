using iOSClub.Data.VOs;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iOSClub.WebAPI.Controllers;

[Authorize(Roles = "Founder")]
[ApiController]
[Route("[controller]")]
public class FounderPermissionController(IStaffRepository staffRepository) : ControllerBase
{
    [HttpGet("members")]
    public async Task<ActionResult<ApiResponse<IEnumerable<StaffVO>>>> Members() => Ok(ApiResponse<IEnumerable<StaffVO>>.Success(await staffRepository.GetPermissionMembersAsync(), "获取权限成员成功"));

    [HttpPost("update-role")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateRole(RoleUpdateRequest request)
    {
        var operatorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? "";
        var result = await staffRepository.UpdateRoleAsync(request.UserId, request.Identity, request.DepartmentName, operatorId);
        return result.Success ? Ok(ApiResponse.Success("权限更新成功")) : Ok(ApiResponse<object>.Fail(ErrorCode.InvalidStatusForOperation, result.Error));
    }
}

public sealed record RoleUpdateRequest(string UserId, string Identity, string? DepartmentName);
