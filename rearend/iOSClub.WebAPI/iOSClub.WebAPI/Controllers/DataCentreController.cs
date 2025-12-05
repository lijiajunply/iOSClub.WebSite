using System.Text;
using iOSClub.Data;
using iOSClub.DataApi.Services;
using iOSClub.WebAPI.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using iOSClub.Data.ShowModels;
using Microsoft.AspNetCore.Authorization;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "Founder, President, Minister")]
[Route("[controller]")] // 使用C#推荐的API路径格式
public class DataCentreController(
    IDbContextFactory<ClubContext> dbContextFactory,
    IDataCentreService dataCentreService)
    : ControllerBase
{
    [HttpGet("year")]
    public async Task<ActionResult<ApiResponse<List<YearCount>>>> GetYearData()
    {
        try
        {
            var yearData = await dataCentreService.GetYearDataAsync();
            return Ok(ApiResponse<List<YearCount>>.Success(yearData));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<List<YearCount>>.Fail(ErrorCode.InternalServerError, "获取年份数据失败"));
        }
    }

    [HttpGet("college")]
    public async Task<ActionResult<ApiResponse<List<AcademyCount>>>> GetCollegeData()
    {
        try
        {
            var collegeData = await dataCentreService.GetCollegeDataAsync();
            return Ok(ApiResponse<List<AcademyCount>>.Success(collegeData));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<List<AcademyCount>>.Fail(ErrorCode.InternalServerError, "获取学院数据失败"));
        }
    }

    [HttpGet("grade")]
    public async Task<ActionResult<ApiResponse<List<GradeCount>>>> GetGradeData()
    {
        try
        {
            var gradeData = await dataCentreService.GetGradeDataAsync();
            return Ok(ApiResponse<List<GradeCount>>.Success(gradeData));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<List<GradeCount>>.Fail(ErrorCode.InternalServerError, "获取年级数据失败"));
        }
    }

    [HttpGet("landscape")]
    public async Task<ActionResult<ApiResponse<List<LandscapeCount>>>> GetLandscapeData()
    {
        try
        {
            var landscapeData = await dataCentreService.GetLandscapeDataAsync();
            return Ok(ApiResponse<List<LandscapeCount>>.Success(landscapeData));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<List<LandscapeCount>>.Fail(ErrorCode.InternalServerError, "获取景观数据失败"));
        }
    }

    [HttpGet("gender")]
    public async Task<ActionResult<ApiResponse<List<GenderCount>>>> GetGenderData()
    {
        try
        {
            var genderData = await dataCentreService.GetGenderDataAsync();
            return Ok(ApiResponse<List<GenderCount>>.Success(genderData));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<List<GenderCount>>.Fail(ErrorCode.InternalServerError, "获取性别数据失败"));
        }
    }


    [HttpPost("update-from-json")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateDataFromJson(IFormFile? file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return Ok(ApiResponse<object>.Fail(ErrorCode.ParameterEmpty, "文件为空"));
            }

            if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(ApiResponse<object>.Fail(ErrorCode.ParameterFormatError, "请上传JSON文件"));
            }

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
                return Ok(ApiResponse<object>.Fail(ErrorCode.ParameterFormatError, "JSON文件格式不正确"));
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

            return Ok(ApiResponse<object>.Success(null, "数据更新成功"));
        }
        catch (JsonException ex)
        {
            return Ok(ApiResponse<object>.Fail(ErrorCode.ParameterFormatError, $"JSON解析错误: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "数据更新失败"));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetCentreData()
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var data = new
            {
                Members = await context.Students.CountAsync(),
                Departments = await context.Departments.CountAsync(),
                Staffs = await context.Staffs.Where(staff => staff.Identity != "Founder").CountAsync(),
                Tasks = await context.Tasks.CountAsync(),
                Projects = await context.Projects.CountAsync(),
                Resources = await context.Resources.CountAsync(),
                Todos = await context.Todos.CountAsync()
            };
            return Ok(ApiResponse<object>.Success(data));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "获取中心数据失败"));
        }
    }

    [HttpGet("export-json")]
    public async Task<IActionResult> ExportJson()
    {
        try
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
        catch (Exception ex)
        {
            // 对于文件下载，保持原有的IActionResult返回类型
            return StatusCode(500, "导出数据失败");
        }
    }
}