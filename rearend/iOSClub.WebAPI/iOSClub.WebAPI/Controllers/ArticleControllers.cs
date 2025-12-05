using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.Common;
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
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleModel>>>> GetArticles()
    {
        try
        {
            var articles = await articleRepository.GetAll();
            return Ok(ApiResponse<IEnumerable<ArticleModel>>.Success(articles.OrderByDescending(x => x.LastWriteTime)));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "获取文章列表时发生错误");
            }
            return Ok(ApiResponse<IEnumerable<ArticleModel>>.Fail(ErrorCode.InternalServerError, "获取文章列表失败"));
        }
    }

    /// <summary>
    /// 根据路径获取文章（公开访问）
    /// </summary>
    [HttpGet("{path}")]
    public async Task<ActionResult<ApiResponse<ArticleModel>>> GetArticle(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Ok(ApiResponse<ArticleModel>.Fail(ErrorCode.ParameterEmpty, "路径不能为空"));
        }

        try
        {
            var userIdentity = "";
            var userJwt = HttpContext.User.GetUser();
            if (userJwt != null) userIdentity = userJwt.Identity;

            var article = await articleRepository.GetFromPath(path, userIdentity);
            return Ok(article == null
                ? ApiResponse<ArticleModel>.Fail(ErrorCode.ArticleNotFound, $"未找到路径为 '{path}' 的文章")
                : ApiResponse<ArticleModel>.Success(article));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "获取文章时发生错误，路径: {Path}", path);
            }
            return Ok(ApiResponse<ArticleModel>.Fail(ErrorCode.InternalServerError, "获取文章失败"));
        }
    }

    /// <summary>
    /// 创建新文章（需要社团成员身份）
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ArticleModel>>> CreateArticle([FromBody] ArticleCreateDto createDto)
    {
        try
        {
            // 数据验证
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(createDto, new ValidationContext(createDto), validationResults, true))
            {
                var errorMessage = string.Join(", ", validationResults.Select(v => v.ErrorMessage));
                return Ok(ApiResponse<ArticleModel>.Fail(ErrorCode.ParameterValidationFailed, errorMessage));
            }

            // 检查路径是否已存在
            var existingArticle = await articleRepository.GetFromPath(createDto.Path);
            if (existingArticle != null)
            {
                return Ok(ApiResponse<ArticleModel>.Fail(ErrorCode.ResourceAlreadyExists,
                    $"路径 '{createDto.Path}' 已存在"));
            }

            var articleModel = new ArticleModel
            {
                Path = createDto.Path,
                Title = createDto.Title,
                Content = createDto.Content,
                Identity = createDto.Identity,
                Category = string.IsNullOrEmpty(createDto.Category)
                    ? null
                    : new CategoryModel() { Name = createDto.Category },
                ArticleOrder = createDto.ArticleOrder,
                LastWriteTime = DateTime.UtcNow
            };

            var success = await articleRepository.CreateOrUpdate(articleModel);
            if (!success)
            {
                return Ok(ApiResponse<ArticleModel>.Fail(ErrorCode.OperationFailed, "创建文章失败"));
            }

            var createdArticle = await articleRepository.GetFromPath(createDto.Path);
            if (createdArticle == null)
            {
                return Ok(ApiResponse<ArticleModel>.Fail(ErrorCode.InternalServerError, "创建文章成功，但获取文章失败"));
            }

            return CreatedAtAction(nameof(GetArticle), new { path = createDto.Path },
                ApiResponse<ArticleModel>.Success(createdArticle, "文章创建成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "创建文章时发生错误");
            }
            return Ok(ApiResponse<ArticleModel>.Fail(ErrorCode.InternalServerError, "创建文章失败"));
        }
    }

    /// <summary>
    /// 更新文章（需要社团成员身份）- 使用POST更安全
    /// </summary>
    [Authorize]
    [HttpPost("update/{path}")]
    public async Task<ActionResult<ApiResponse>> UpdateArticle(string path, [FromBody] ArticleUpdateDto updateDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Ok(ApiResponse.Fail(ErrorCode.ParameterEmpty, "路径不能为空"));
            }

            // 验证更新数据
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(updateDto, new ValidationContext(updateDto), validationResults, true))
            {
                var errorMessage = string.Join(", ", validationResults.Select(v => v.ErrorMessage));
                return Ok(ApiResponse.Fail(ErrorCode.ParameterValidationFailed, errorMessage));
            }

            // 检查文章是否存在
            var existingArticle = await articleRepository.GetFromPath(path);
            if (existingArticle == null)
            {
                return Ok(ApiResponse.Fail(ErrorCode.ArticleNotFound, $"未找到路径为 '{path}' 的文章"));
            }

            // 更新文章信息
            existingArticle.Title = updateDto.Title;
            existingArticle.Content = updateDto.Content;
            existingArticle.Identity = updateDto.Identity;
            existingArticle.Category = string.IsNullOrEmpty(updateDto.Category)
                ? null
                : new CategoryModel() { Name = updateDto.Category };
            existingArticle.ArticleOrder = updateDto.ArticleOrder;
            existingArticle.LastWriteTime = DateTime.UtcNow;

            var success = await articleRepository.CreateOrUpdate(existingArticle);
            return !success
                ? Ok(ApiResponse.Fail(ErrorCode.OperationFailed, "更新文章失败"))
                : Ok(ApiResponse.Success("文章更新成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "更新文章时发生错误，路径: {Path}", path);
            }
            return Ok(ApiResponse.Fail(ErrorCode.InternalServerError, "更新文章失败"));
        }
    }

    /// <summary>
    /// 删除文章（需要管理员身份）
    /// </summary>
    [Authorize]
    [HttpPost("delete/{path}")]
    public async Task<ActionResult<ApiResponse>> DeleteArticle(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Ok(ApiResponse.Fail(ErrorCode.ParameterEmpty, "路径不能为空"));
            }

            // 检查文章是否存在
            var existingArticle = await articleRepository.GetFromPath(path);
            if (existingArticle == null)
            {
                return Ok(ApiResponse.Fail(ErrorCode.ArticleNotFound, $"未找到路径为 '{path}' 的文章"));
            }

            var success = await articleRepository.Delete(path);
            if (!success)
            {
                return Ok(ApiResponse.Fail(ErrorCode.OperationFailed, "删除文章失败"));
            }

            return Ok(ApiResponse.Success("文章删除成功"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "删除文章时发生错误，路径: {Path}", path);
            }
            return Ok(ApiResponse.Fail(ErrorCode.InternalServerError, "删除文章失败"));
        }
    }

    /// <summary>
    /// 搜索文章并返回高亮片段（公开访问）
    /// </summary>
    [HttpGet("search/highlights")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleSearchResult>>>> SearchArticlesWithHighlights(
        [FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return Ok(ApiResponse<IEnumerable<ArticleSearchResult>>.Fail(ErrorCode.ParameterEmpty, "搜索关键词不能为空"));
        }

        try
        {
            var userIdentity = "";
            var userJwt = HttpContext.User.GetUser();
            if (userJwt != null) userIdentity = userJwt.Identity;

            var articles = await articleRepository.SearchArticlesWithHighlights(keyword, userIdentity);
            return Ok(ApiResponse<IEnumerable<ArticleSearchResult>>.Success(
                articles.OrderByDescending(a => a.LastWriteTime)));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "搜索文章时发生错误，关键词: {Keyword}", keyword);
            }
            return Ok(ApiResponse<IEnumerable<ArticleSearchResult>>.Fail(ErrorCode.InternalServerError, "搜索文章失败"));
        }
    }

    /// <summary>
    /// 获取文章分类列表
    /// </summary>
    [HttpGet("category")]
    public async Task<ActionResult<ApiResponse<object>>> GetAllCategoryArticles()
    {
        try
        {
            var userIdentity = "";
            var userJwt = HttpContext.User.GetUser();
            if (userJwt != null) userIdentity = userJwt.Identity;

            var articles = await articleRepository.GetAllCategoryArticles(userIdentity);
            return Ok(ApiResponse<object>.Success(articles));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "获取文章分类列表时发生错误");
            }
            return Ok(ApiResponse<object>.Fail(ErrorCode.InternalServerError, "获取文章分类列表失败"));
        }
    }

    /// <summary>
    /// 批量更新文章顺序（需要社团成员身份）
    /// </summary>
    [Authorize]
    [HttpPost("update-orders")]
    public async Task<ActionResult<ApiResponse>> UpdateArticleOrders([FromBody] Dictionary<string, int>? articleOrders)
    {
        try
        {
            if (articleOrders == null || articleOrders.Count == 0)
            {
                return Ok(ApiResponse.Fail(ErrorCode.ParameterEmpty, "文章顺序字典不能为空"));
            }

            var success = await articleRepository.UpdateArticleOrders(articleOrders);
            if (success)
            {
                return Ok(ApiResponse.Success("文章顺序更新成功"));
            }

            return Ok(ApiResponse.Fail(ErrorCode.OperationFailed, "文章顺序更新失败"));
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(ex, "批量更新文章顺序时发生错误");
            }
            return Ok(ApiResponse.Fail(ErrorCode.InternalServerError, "文章顺序更新失败"));
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

    [StringLength(128, ErrorMessage = "分类长度不能超过128个字符")]
    public string? Category { get; set; }

    [Range(0, 1000, ErrorMessage = "文章排序值必须在0-1000之间")]
    public int ArticleOrder { get; set; } = 0;
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

    [StringLength(128, ErrorMessage = "分类长度不能超过128个字符")]
    public string? Category { get; set; }

    [Range(0, 1000, ErrorMessage = "文章排序值必须在0-1000之间")]
    public int ArticleOrder { get; set; } = 0;
}