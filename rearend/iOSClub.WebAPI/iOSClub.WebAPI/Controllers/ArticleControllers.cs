using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")] // 使用C#推荐的API路径格式
public class ArticleController(
    IArticleRepository articleRepository,
    ILogger<ArticleController> logger)
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
            var userIdentity = "";
            var userJwt = HttpContext.User.GetUser();
            if (userJwt != null) userIdentity = userJwt.Identity;

            var article = await articleRepository.GetFromPath(path, userIdentity);
            if (article == null)
            {
                return NotFound($"未找到路径为 '{path}' 的文章");
            }

            return article;
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
    [HttpPost]
    public async Task<ActionResult<ArticleModel>> CreateArticle([FromBody] ArticleCreateDto createDto)
    {
        try
        {
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
    [HttpPost("update/{path}")]
    public async Task<ActionResult> UpdateArticle(string path, [FromBody] ArticleUpdateDto updateDto)
    {
        try
        {
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
            return !success ? StatusCode(500, "更新文章失败") : Ok(new { message = "文章更新成功", path });
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
    [HttpPost("delete/{path}")]
    public async Task<ActionResult> DeleteArticle(string path)
    {
        try
        {
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
    /// 获取文章分类列表
    /// </summary>
    [HttpGet("category")]
    public async Task<IActionResult> GetAllCategoryArticles()
    {
        try
        {
            var userIdentity = "";
            var userJwt = HttpContext.User.GetUser();
            if (userJwt != null) userIdentity = userJwt.Identity;

            var articles = await articleRepository.GetAllCategoryArticles(userIdentity);
            return Ok(articles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取文章分类列表时发生错误");
            return StatusCode(500, "服务器内部错误");
        }
    }
}

// 创建文章的DTO
[Serializable]
public class ArticleCreateDto(string? identity)
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
    public string? Identity { get; set; } = identity;
}

// 更新文章的DTO
[Serializable]
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