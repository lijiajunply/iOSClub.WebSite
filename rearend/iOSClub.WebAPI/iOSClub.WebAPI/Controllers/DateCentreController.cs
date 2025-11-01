using iOSClub.Data;
using iOSClub.DataApi.ShowModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]  // 使用C#推荐的API路径格式
public class DateCentreController(IDbContextFactory<iOSContext> dbContextFactory) : ControllerBase
{
    [HttpGet]
    public ActionResult<string> GetNeedData()
    {
        return "Hello World!";
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateNeedData([FromBody] AllDataModel allData)
    {
        foreach (var student in allData.Students.Where(student => student.JoinTime.Year == 0))
        {
            student.JoinTime = DateTime.Parse("2023-10-18");
        }

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

        return Ok();
    }

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
            foreach (var student in allData.Students.Where(student => student.JoinTime.Year == 0))
            {
                student.JoinTime = DateTime.Parse("2023-10-18");
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
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器内部错误: {ex.Message}");
        }
    }
}