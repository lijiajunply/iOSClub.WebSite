using System.Text;
using iOSClub.Data;
using iOSClub.DataApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using iOSClub.Data.ShowModels;
using Microsoft.AspNetCore.Authorization;
using StackExchange.Redis;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")] // 使用C#推荐的API路径格式
public class DataCentreController(
    IDbContextFactory<ClubContext> dbContextFactory,
    IDataCentreService dataCentreService,
    IConnectionMultiplexer redis)
    : ControllerBase
{
    private readonly IDatabase _db = redis.GetDatabase();

    [HttpGet("year")]
    public async Task<ActionResult<List<YearCount>>> GetYearData()
    {
        var yearData = await dataCentreService.GetYearDataAsync();
        return Ok(yearData);
    }

    [HttpGet("college")]
    public async Task<ActionResult<List<AcademyCount>>> GetCollegeData()
    {
        var collegeData = await dataCentreService.GetCollegeDataAsync();
        return Ok(collegeData);
    }

    [HttpGet("grade")]
    public async Task<ActionResult<List<LandscapeCount>>> GetGradeData()
    {
        var gradeData = await dataCentreService.GetGradeDataAsync();
        return Ok(gradeData);
    }

    [HttpGet("landscape")]
    public async Task<ActionResult<List<LandscapeCount>>> GetLandscapeData()
    {
        var landscapeData = await dataCentreService.GetLandscapeDataAsync();
        return Ok(landscapeData);
    }

    [HttpGet("gender")]
    public async Task<ActionResult<List<GenderCount>>> GetGenderData()
    {
        var genderData = await dataCentreService.GetGenderDataAsync();
        return Ok(genderData);
    }

    [Authorize(Roles = "Founder, President, Minister")]
    [HttpPost("update-from-json")]
    public async Task<IActionResult> UpdateDataFromJson(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("文件为空");
        }

        if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("请上传JSON文件");
        }

        try
        {
            // 读取文件内容
            using var reader = new StreamReader(file.OpenReadStream());
            var jsonContent = await reader.ReadToEndAsync();

            // 反序列化JSON到AllDataModel
            var allData = JsonSerializer.Deserialize<AllDataModel>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (allData == null)
            {
                return BadRequest("JSON文件格式不正确");
            }

            // 处理学生数据，与现有方法保持一致
            foreach (var student in allData.Students)
            {
                student.JoinTime =
                    DateTime.SpecifyKind(student.JoinTime.Year == 0 ? DateTime.Parse("2023-10-18") : student.JoinTime,
                        DateTimeKind.Utc);
            }

            // 确保所有DateTime值都是UTC时间
            foreach (var todo in allData.Todos.Where(todo => todo.CreatedTime.Kind == DateTimeKind.Unspecified))
            {
                todo.CreatedTime = DateTime.SpecifyKind(todo.CreatedTime, DateTimeKind.Utc);
            }

            foreach (var article in allData.Articles.Where(article =>
                         article.LastWriteTime.Kind == DateTimeKind.Unspecified))
            {
                article.LastWriteTime = DateTime.SpecifyKind(article.LastWriteTime, DateTimeKind.Utc);
            }

            // 使用相同的数据更新逻辑
            await using var context = await dbContextFactory.CreateDbContextAsync();

            await context.Students.AddRangeAsync(allData.Students);
            await context.SaveChangesAsync();
            await context.Departments.AddRangeAsync(allData.Departments);
            await context.Staffs.AddRangeAsync(allData.Presidents.Where(staff => staff.Identity == "President"));
            await context.SaveChangesAsync();
            await context.Tasks.AddRangeAsync(allData.Tasks);
            await context.Projects.AddRangeAsync(allData.Projects);
            await context.Resources.AddRangeAsync(allData.Resources);
            await context.Todos.AddRangeAsync(allData.Todos);
            await context.Articles.AddRangeAsync(allData.Articles);
            await context.SaveChangesAsync();

            return Ok(new { message = "数据更新成功" });
        }
        catch (JsonException ex)
        {
            return BadRequest($"JSON解析错误: {ex.Message}");
        }
    }

    [Authorize(Roles = "Founder, President, Minister")]
    [HttpGet]
    public async Task<IActionResult> GetCentreData()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return Ok(new
        {
            Members = await context.Students.CountAsync(),
            Departments = await context.Departments.CountAsync(),
            Staffs = await context.Staffs.Where(staff => staff.Identity != "Founder").CountAsync(),
            Tasks = await context.Tasks.CountAsync(),
            Projects = await context.Projects.CountAsync(),
            Resources = await context.Resources.CountAsync(),
            Todos = await context.Todos.CountAsync()
        });
    }

    [Authorize(Roles = "Founder, President, Minister")]
    [HttpGet("export-json")]
    public async Task<IActionResult> ExportJson()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var allData = new AllDataModel
        {
            Students = await context.Students.ToListAsync(),
            Departments = await context.Departments.ToListAsync(),
            Presidents = await context.Staffs.Where(staff => staff.Identity == "President").ToListAsync(),
            Tasks = await context.Tasks.ToListAsync(),
            Projects = await context.Projects.ToListAsync(),
            Resources = await context.Resources.ToListAsync(),
            Todos = await context.Todos.ToListAsync(),
        };

        var options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
            WriteIndented = true
        };

        return File(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(allData, options)), "application/json",
            "all-data.json");
    }

    [HttpGet("clean")]
    public async Task<IActionResult> CleanData()
    {
        // 清除所有缓存
        var deletedKeys = await _db.KeyDeleteAsync("*");
        return deletedKeys? BadRequest("出现问题") : Ok("缓存已清除");
    }
}