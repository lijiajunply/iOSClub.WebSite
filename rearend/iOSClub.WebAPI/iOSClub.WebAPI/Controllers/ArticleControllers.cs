﻿using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using iOSClub.Data;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]  // 使用C#推荐的API路径格式
public class ArticleController(
    IArticleRepository articleRepository,
    ILogger<ArticleController> logger,
    IDbContextFactory<iOSContext> factory,
    IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    /// <summary>
    /// 获取所有文章（公开访问）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArticleModel>>> GetArticles()
    {
        try
        {
            var articles = await articleRepository.GetAll();
            return Ok(articles.OrderBy(x => x.LastWriteTime));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取文章列表时发生错误");
            return StatusCode(500, "服务器内部错误");
        }
    }

    /// <summary>
    /// 根据路径获取文章（公开访问）
    /// </summary>
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

    /// <summary>
    /// 创建新文章（需要社团成员身份）
    /// </summary>
    [Authorize]
    [TokenActionFilter]
    [HttpPost]
    public async Task<ActionResult<ArticleModel>> CreateArticle([FromBody] ArticleCreateDto createDto)
    {
        try
        {
            // 身份验证和权限检查
            var validationResult = await ValidateUserAccess(["Founder", "President", "Minister", "Department"]);
            if (!validationResult.isValid)
                return validationResult.errorResult!;

            // 数据验证
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(createDto, new ValidationContext(createDto), validationResults, true))
            {
                return BadRequest(validationResults.Select(v => v.ErrorMessage));
            }

            // 检查路径是否已存在
            var existingArticle = await articleRepository.GetFromPath(createDto.Path);
            if (existingArticle != null)
            {
                return Conflict($"路径 '{createDto.Path}' 已存在");
            }

            var articleModel = new ArticleModel
            {
                Path = createDto.Path,
                Title = createDto.Title,
                Content = createDto.Content,
                Identity = createDto.Identity,
                LastWriteTime = DateTime.UtcNow
            };

            var success = await articleRepository.CreateOrUpdate(articleModel);
            if (!success)
            {
                return StatusCode(500, "创建文章失败");
            }

            var createdArticle = await articleRepository.GetFromPath(createDto.Path);
            return CreatedAtAction(nameof(GetArticle), new { path = createDto.Path }, createdArticle);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "创建文章时发生错误");
            return StatusCode(500, "服务器内部错误");
        }
    }

    /// <summary>
    /// 更新文章（需要社团成员身份）- 使用POST更安全
    /// </summary>
    [Authorize]
    [TokenActionFilter]
    [HttpPost("update/{path}")]
    public async Task<ActionResult> UpdateArticle(string path, [FromBody] ArticleUpdateDto updateDto)
    {
        try
        {
            // 身份验证和权限检查
            var validationResult = await ValidateUserAccess(["Founder", "President", "Minister", "Department"]);
            if (!validationResult.isValid)
                return validationResult.errorResult!;

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
            existingArticle.LastWriteTime = DateTime.UtcNow;

            var success = await articleRepository.CreateOrUpdate(existingArticle);
            if (!success)
            {
                return StatusCode(500, "更新文章失败");
            }

            return Ok(new { message = "文章更新成功", path });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新文章时发生错误，路径: {Path}", path);
            return StatusCode(500, "服务器内部错误");
        }
    }

    /// <summary>
    /// 删除文章（需要管理员身份）
    /// </summary>
    [Authorize]
    [TokenActionFilter]
    [HttpPost("delete/{path}")]
    public async Task<ActionResult> DeleteArticle(string path)
    {
        try
        {
            // 身份验证和权限检查（管理员权限）
            var validationResult = await ValidateUserAccess(["Founder", "President", "Minister"]);
            if (!validationResult.isValid)
                return validationResult.errorResult!;

            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("路径不能为空");
            }

            // 检查文章是否存在
            var existingArticle = await articleRepository.GetFromPath(path);
            if (existingArticle == null)
            {
                return NotFound($"未找到路径为 '{path}' 的文章");
            }

            var success = await articleRepository.Delete(path);
            if (!success)
            {
                return StatusCode(500, "删除文章失败");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "删除文章时发生错误，路径: {Path}", path);
            return StatusCode(500, "服务器内部错误");
        }
    }

    /// <summary>
    /// 搜索文章（公开访问）
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ArticleModel>>> SearchArticles([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest("搜索关键词不能为空");
        }

        try
        {
            var articles = await articleRepository.GetAll();
            var filteredArticles = articles
                .Where(a => a.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                           a.Content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(a => a.LastWriteTime)
                .ToList();

            return Ok(filteredArticles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "搜索文章时发生错误，关键词: {Keyword}", keyword);
            return StatusCode(500, "服务器内部错误");
        }
    }

    /// <summary>
    /// 验证用户访问权限
    /// </summary>
    private async Task<(bool isValid, ActionResult? errorResult)> ValidateUserAccess(string[] allowedIdentities)
    {
        var userJwt = httpContextAccessor.HttpContext?.User.GetUser();
        if (userJwt == null)
            return (false, Unauthorized("用户未认证"));

        await using var context = await factory.CreateDbContextAsync();
        var user = await context.Staffs
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.UserId == userJwt.UserId);

        if (user == null)
            return (false, Unauthorized("用户不存在"));

        if (!allowedIdentities.Contains(user.Identity))
            return (false, Forbid("权限不足"));

        return (true, null);
    }
}

// 创建文章的DTO
public class ArticleCreateDto
{
    [Required(ErrorMessage = "文章路径是必需的")]
    [StringLength(128, ErrorMessage = "路径长度不能超过128个字符")]
    [RegularExpression("^[a-zA-Z0-9_-]+$", ErrorMessage = "路径只能包含字母、数字、下划线和连字符")]
    public string Path { get; set; } = "";

    [Required(ErrorMessage = "文章标题是必需的")]
    [StringLength(100, ErrorMessage = "标题长度不能超过100个字符")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "文章内容是必需的")]
    [MinLength(10, ErrorMessage = "内容至少需要10个字符")]
    public string Content { get; set; } = "";

    [StringLength(20, ErrorMessage = "身份标识长度不能超过20个字符")]
    public string? Identity { get; set; }
}

// 更新文章的DTO
public class ArticleUpdateDto
{
    [Required(ErrorMessage = "文章标题是必需的")]
    [StringLength(100, ErrorMessage = "标题长度不能超过100个字符")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "文章内容是必需的")]
    [MinLength(10, ErrorMessage = "内容至少需要10个字符")]
    public string Content { get; set; } = "";

    [StringLength(20, ErrorMessage = "身份标识长度不能超过20个字符")]
    public string? Identity { get; set; }
}