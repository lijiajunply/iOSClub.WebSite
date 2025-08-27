using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using iOSClub.Data;

namespace iOSClub.WebAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class ArticleController(
    IArticleRepository articleRepository,
    ILogger<ArticleController> logger,
    IDbContextFactory<iOSContext> factory,
    IHttpContextAccessor httpContextAccessor) // 添加身份验证所需的依赖
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArticleModel>>> GetArticles()
    {
        try
        {
            var articles = await articleRepository.GetAll();
            return Ok(articles.OrderBy(x => x.LastWriteTime)); // 保持排序逻辑
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取文章列表时发生错误");
            return StatusCode(500, "服务器内部错误");
        }
    }

    [HttpGet("{path}")]
    public async Task<ActionResult<ArticleModel>> GetArticle(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest("路径不能为空");
        }

        try
        {
            var article = await articleRepository.GetFromPath(path);
            if (article == null)
            {
                return NotFound($"未找到路径为 '{path}' 的文章");
            }

            return Ok(article);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取文章时发生错误，路径: {Path}", path);
            return StatusCode(500, "服务器内部错误");
        }
    }

    [Authorize]
    [TokenActionFilter]
    [HttpPost]
    public async Task<ActionResult<ArticleModel>> CreateArticle([FromBody] ArticleCreateDto createDto)
    {
        // 身份验证
        var userJwt = httpContextAccessor.HttpContext?.User.GetUser();
        if (userJwt is not { Identity: "Founder" or "President" or "Minister" or "Department" })
            return Unauthorized("权限不足");

        await using var context = await factory.CreateDbContextAsync();
        var user = await context.Staffs.Include(x => x.Department).FirstOrDefaultAsync(x => x.UserId == userJwt.UserId);

        if (user is not { Identity: "Founder" or "President" or "Minister" or "Department" })
            return Unauthorized("用户数据错误");

        // 数据验证
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(createDto, new ValidationContext(createDto), validationResults, true))
        {
            return BadRequest(validationResults.Select(v => v.ErrorMessage));
        }

        try
        {
            var articleModel = new ArticleModel
            {
                Path = createDto.Path,
                Title = createDto.Title,
                Content = createDto.Content,
                Identity = createDto.Identity,
                LastWriteTime = DateTime.Now
            };

            var success = await articleRepository.CreateOrUpdate(articleModel);
            if (!success)
            {
                return StatusCode(500, "创建文章失败");
            }

            // 返回创建后的文章信息
            var createdArticle = await articleRepository.GetFromPath(createDto.Path);
            return CreatedAtAction(nameof(GetArticle), new { path = createDto.Path }, createdArticle);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "创建文章时发生错误");
            return StatusCode(500, "服务器内部错误");
        }
    }

    [Authorize]
    [TokenActionFilter]
    [HttpPut("{path}")]
    public async Task<ActionResult> UpdateArticle(string path, [FromBody] ArticleUpdateDto updateDto)
    {
        // 身份验证
        var userJwt = httpContextAccessor.HttpContext?.User.GetUser();
        if (userJwt is not { Identity: "Founder" or "President" or "Minister" or "Department" })
            return Unauthorized("权限不足");

        await using var context = await factory.CreateDbContextAsync();
        var user = await context.Staffs.Include(x => x.Department).FirstOrDefaultAsync(x => x.UserId == userJwt.UserId);

        if (user is not { Identity: "Founder" or "President" or "Minister" or "Department" })
            return Unauthorized("用户数据错误");

        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest("路径不能为空");
        }

        // 验证更新数据
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(updateDto, new ValidationContext(updateDto), validationResults, true))
        {
            return BadRequest(validationResults.Select(v => v.ErrorMessage));
        }

        try
        {
            // 检查文章是否存在
            var existingArticle = await articleRepository.GetFromPath(path);
            if (existingArticle == null)
            {
                return NotFound($"未找到路径为 '{path}' 的文章");
            }

            // 更新文章信息
            existingArticle.Title = updateDto.Title;
            existingArticle.Content = updateDto.Content;
            existingArticle.Identity = updateDto.Identity;
            existingArticle.LastWriteTime = DateTime.Now;

            var success = await articleRepository.CreateOrUpdate(existingArticle);
            if (!success)
            {
                return StatusCode(500, "更新文章失败");
            }

            return NoContent(); // 204 No Content 是更新操作的标准响应
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新文章时发生错误，路径: {Path}", path);
            return StatusCode(500, "服务器内部错误");
        }
    }

    [Authorize]
    [TokenActionFilter]
    [HttpDelete("{path}")]
    public async Task<ActionResult> DeleteArticle(string path)
    {
        // 身份验证
        var userJwt = httpContextAccessor.HttpContext?.User.GetUser();
        if (userJwt is not { Identity: "Founder" or "President" or "Minister" or "Department" })
            return Unauthorized("权限不足");

        await using var context = await factory.CreateDbContextAsync();
        var user = await context.Staffs.Include(x => x.Department).FirstOrDefaultAsync(x => x.UserId == userJwt.UserId);

        if (user is not { Identity: "Founder" or "President" or "Minister" or "Department" })
            return Unauthorized("用户数据错误");

        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest("路径不能为空");
        }

        try
        {
            var success = await articleRepository.Delete(path);
            if (!success)
            {
                return NotFound($"未找到路径为 '{path}' 的文章");
            }

            return NoContent(); // 204 No Content 是删除操作的标准响应
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "删除文章时发生错误，路径: {Path}", path);
            return StatusCode(500, "服务器内部错误");
        }
    }
}

// 创建文章的DTO（数据传输对象）
public class ArticleCreateDto
{
    [Required(ErrorMessage = "文章路径是必需的")]
    [StringLength(128, ErrorMessage = "路径长度不能超过128个字符")]
    public string Path { get; set; } = "";

    [Required(ErrorMessage = "文章标题是必需的")]
    [StringLength(32, ErrorMessage = "标题长度不能超过32个字符")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "文章内容是必需的")]
    public string Content { get; set; } = "";

    [StringLength(20, ErrorMessage = "身份标识长度不能超过20个字符")]
    public string? Identity { get; set; }
}

// 更新文章的DTO
public class ArticleUpdateDto
{
    [Required(ErrorMessage = "文章标题是必需的")]
    [StringLength(32, ErrorMessage = "标题长度不能超过32个字符")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "文章内容是必需的")]
    public string Content { get; set; } = "";

    [StringLength(20, ErrorMessage = "身份标识长度不能超过20个字符")]
    public string? Identity { get; set; }
}