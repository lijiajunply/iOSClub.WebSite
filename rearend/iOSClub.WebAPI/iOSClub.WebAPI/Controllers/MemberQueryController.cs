using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iOSClub.WebAPI.Controllers;

/// <summary>
/// 成员查询控制器 - 负责成员数据的查询功能
/// </summary>
[Authorize(Roles = "Founder, President, Minister")]
[TokenActionFilter]
[ApiController]
[Route("[controller]")]
public class MemberQueryController(IStudentRepository studentRepository) : ControllerBase
{
    /// <summary>
    /// 获取所有成员数据
    /// </summary>
    /// <returns>压缩后的成员数据JSON字符串</returns>
    [HttpGet("all-data")]
    public async Task<ActionResult<string>> GetAllData()
    {
        var members = await studentRepository.GetAllMembersAsync();
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        return GZipServer.CompressString(JsonConvert.SerializeObject(members, settings));
    }

    /// <summary>
    /// 分页获取所有成员数据
    /// </summary>
    /// <param name="pageNum">页码，默认1</param>
    /// <param name="pageSize">每页大小，默认10</param>
    /// <returns>分页后的成员数据</returns>
    [HttpGet("all-data/page")]
    public async Task<ActionResult<string>> GetAllDataByPage(int pageNum = 1, int pageSize = 10)
    {
        if (pageNum < 1 || pageSize < 1 || pageSize > 100) // 限制最大页大小
        {
            return BadRequest("Invalid pagination parameters");
        }

        var (members, totalCount) = await studentRepository.GetMembersPagedAsync(pageNum, pageSize);
        
        var response = new
        {
            TotalCount = totalCount,
            PageSize = pageSize,
            CurrentPage = pageNum,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Data = members
        };

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        return GZipServer.CompressString(JsonConvert.SerializeObject(response, settings));
    }
}