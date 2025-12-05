using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iOSClub.WebAPI.Controllers;

/// <summary>
/// 成员查询控制器 - 负责成员数据的查询功能
/// </summary>
[Authorize(Roles = "Founder, President, Minister")]
[ApiController]
[Route("[controller]")]
public class MemberQueryController(IStudentRepository studentRepository, ILogger<MemberQueryController> logger) : ControllerBase
{
    /// <summary>
    /// 获取所有成员数据
    /// </summary>
    /// <returns>压缩后的成员数据JSON字符串</returns>
    [HttpGet("all-data")]
    public async Task<ActionResult<ApiResponse<string>>> GetAllData()
    {
        try
        {
            var members = await studentRepository.GetAllMembersAsync();
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var result = GZipServer.CompressString(JsonConvert.SerializeObject(members, settings));
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("获取所有成员数据成功，成员数量: {Count}", members.Count);
            }
            return Ok(ApiResponse<string>.Success(result, "获取所有成员数据成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "获取所有成员数据失败");
            }
            return Ok(ApiResponse<string>.Fail(ErrorCode.InternalServerError, "获取所有成员数据失败"));
        }
    }

    /// <summary>
    /// 分页获取所有成员数据（支持搜索）
    /// </summary>
    /// <param name="pageNum">页码，默认1</param>
    /// <param name="pageSize">每页大小，默认10</param>
    /// <param name="searchTerm">搜索词</param>
    /// <param name="searchCondition">搜索条件</param>
    /// <returns>分页后的成员数据</returns>
    [HttpGet("all-data/page")]
    public async Task<ActionResult<ApiResponse<string>>> GetAllDataByPage(int pageNum = 1, int pageSize = 10,
        string? searchTerm = null, string? searchCondition = null)
    {
        try
        {
            if (pageNum < 1 || pageSize < 1 || pageSize > 100) // 限制最大页大小
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("无效的分页参数，页码: {PageNum}, 页大小: {PageSize}", pageNum, pageSize);
                }
                return Ok(ApiResponse<string>.Fail(ErrorCode.ParameterOutOfRange, "无效的分页参数"));
            }

            var (members, totalCount) = 
                await studentRepository.GetMembersPagedAsync(pageNum, pageSize, searchTerm, searchCondition);

            var response = new
            {
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNum,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Data = members
            };

            var settingsWithCamelCase = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var result = GZipServer.CompressString(JsonConvert.SerializeObject(response, settingsWithCamelCase));
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("分页获取成员数据成功，页码: {PageNum}, 页大小: {PageSize}, 总数量: {TotalCount}", pageNum, pageSize, totalCount);
            }
            return Ok(ApiResponse<string>.Success(result, "分页获取成员数据成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "分页获取成员数据失败，页码: {PageNum}, 页大小: {PageSize}", pageNum, pageSize);
            }
            return Ok(ApiResponse<string>.Fail(ErrorCode.InternalServerError, "分页获取成员数据失败"));
        }
    }

    /// <summary>
    /// 搜索
    /// </summary>
    /// <param name="searchTerm">搜索词</param>
    /// <param name="searchCondition">搜索条件</param>
    /// <returns>分页后的成员数据</returns>
    [HttpGet("all-data/search")]
    public async Task<ActionResult<ApiResponse<object>>> Search(string searchTerm, string searchCondition)
    {
        try
        {
            var data = await studentRepository.Search(searchTerm, searchCondition);
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("搜索成员数据成功，搜索词: {SearchTerm}, 搜索条件: {SearchCondition}", searchTerm, searchCondition);
            }
            return Ok(ApiResponse<object>.Success(data, "搜索成员数据成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "搜索成员数据失败，搜索词: {SearchTerm}, 搜索条件: {SearchCondition}", searchTerm, searchCondition);
            }
            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "搜索成员数据失败"));
        }
    }
}