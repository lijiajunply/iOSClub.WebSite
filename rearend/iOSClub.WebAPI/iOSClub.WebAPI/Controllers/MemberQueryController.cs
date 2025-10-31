using iOSClub.Data;
using iOSClub.Data.ShowModels;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace iOSClub.WebAPI.Controllers;

/// <summary>
/// 成员查询控制器 - 负责成员数据的查询功能
/// </summary>
[Authorize(Roles = "Founder, President, Minister")]
[TokenActionFilter]
[ApiController]
[Route("[controller]")]
public class MemberQueryController(IDbContextFactory<iOSContext> factory) : ControllerBase
{
    /// <summary>
    /// 获取所有成员数据
    /// </summary>
    /// <returns>压缩后的成员数据JSON字符串</returns>
    [HttpGet("all-data")]
    public async Task<ActionResult<string>> GetAllData()
    {
        await using var context = await factory.CreateDbContextAsync();

        var query = from student in context.Students
                   join staff in context.Staffs
                       on student.UserId equals staff.UserId into staffGroup
                   from staff in staffGroup.DefaultIfEmpty() // LEFT JOIN
                   select MemberModel.CopyFrom(student, staff != null ? staff.Identity : "Member");
        
        var members = await query.ToListAsync();
        return GZipServer.CompressString(JsonConvert.SerializeObject(members));
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

        await using var context = await factory.CreateDbContextAsync();

        // 优化JOIN查询：先获取分页后的学生数据，再一次性关联员工数据
        var skipCount = (pageNum - 1) * pageSize;
        var studentIdsQuery = context.Students
            .OrderBy(s => s.UserId) // 确保结果一致性的排序
            .Skip(skipCount)
            .Take(pageSize)
            .Select(s => s.UserId);

        var studentIds = await studentIdsQuery.ToListAsync(); // 使用两个更小的查询代替一个复杂查询
        var studentsQuery = context.Students
            .Where(s => studentIds.Contains(s.UserId))
            .AsNoTracking(); // 禁用变更跟踪提高性能
        var staffQuery = context.Staffs
            .Where(s => studentIds.Contains(s.UserId))
            .AsNoTracking(); // 并行执行两个查询

        var totalCount = await context.Students.CountAsync();
        var students = await studentsQuery.ToListAsync();
        var staffs = await staffQuery.ToListAsync();

        // 在内存中执行连接操作
        var staffMap = staffs.ToDictionary(s => s.UserId);

        var results = students.Select(student =>
        {
            staffMap.TryGetValue(student.UserId, out var staff);
            return MemberModel.CopyFrom(student, staff != null ? staff.Identity : "Member");
        }).ToList();

        var response = new
        {
            TotalCount = totalCount,
            PageSize = pageSize,
            CurrentPage = pageNum,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Data = results
        };

        return GZipServer.CompressString(JsonConvert.SerializeObject(response));
    }

    /// <summary>
    /// 分页获取员工数据
    /// </summary>
    /// <param name="pageNum">页码，默认1</param>
    /// <param name="pageSize">每页大小，默认10</param>
    /// <returns>分页后的员工数据</returns>
    [HttpGet("staffs/page")]
    public async Task<ActionResult<string>> GetStaffsByPage(int pageNum = 1, int pageSize = 10)
    {
        if (pageNum < 1 || pageSize < 1 || pageSize > 100) // 限制最大页大小
        {
            return BadRequest("Invalid pagination parameters");
        }

        await using var context = await factory.CreateDbContextAsync();

        var skipCount = (pageNum - 1) * pageSize;
        var studentQuery = context.Staffs
            .OrderBy(s => s.UserId) // 确保结果一致性的排序
            .Skip(skipCount)
            .Take(pageSize)
            .Select(x => new { x.UserId, x.Identity });

        var re = from student in studentQuery
                join studentModel in context.Students on student.UserId equals studentModel.UserId
                select MemberModel.CopyFrom(studentModel, student.Identity);

        var totalCount = await context.Students.CountAsync();

        var response = new
        {
            TotalCount = totalCount,
            PageSize = pageSize,
            CurrentPage = pageNum,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Data = await re.ToListAsync()
        };

        return GZipServer.CompressString(JsonConvert.SerializeObject(response));
    }
}