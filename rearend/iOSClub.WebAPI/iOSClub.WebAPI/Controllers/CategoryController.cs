using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")] // 使用C#推荐的API路径格式
public class CategoryController(ICategoryRepository categoryRepository, ILogger<CategoryController> logger)
    : ControllerBase
{
    /// <summary>
    /// 获取所有分类（公开访问）
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryModel>>>> GetAllCategories()
    {
        try
        {
            var categories = await categoryRepository.GetAll();
            return Ok(ApiResponse<IEnumerable<CategoryModel>>.Success(categories));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取分类列表时发生错误");
            return Ok(ApiResponse<IEnumerable<CategoryModel>>.Fail(ErrorCode.InternalServerError, "获取分类列表失败"));
        }
    }

    /// <summary>
    /// 根据名称获取分类（公开访问）
    /// </summary>
    [HttpGet("{name}")]
    public async Task<ActionResult<ApiResponse<CategoryModel>>> GetCategory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Ok(ApiResponse<CategoryModel>.Fail(ErrorCode.ParameterEmpty, "分类名称不能为空"));
        }

        try
        {
            var category = await categoryRepository.GetByName(name);
            if (category == null)
            {
                return Ok(ApiResponse<CategoryModel>.Fail(ErrorCode.CategoryNotFound, $"未找到名称为 '{name}' 的分类"));
            }

            return Ok(ApiResponse<CategoryModel>.Success(category));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取分类时发生错误，名称: {Name}", name);
            return Ok(ApiResponse<CategoryModel>.Fail(ErrorCode.InternalServerError, "获取分类失败"));
        }
    }

    [HttpGet("byId/{id}")]
    public async Task<ActionResult<ApiResponse<CategoryModel>>> GetCategoryById(string id)
    {
        try
        {
            var category = await categoryRepository.GetById(id);
            if (category == null)
            {
                return Ok(ApiResponse<CategoryModel>.Fail(ErrorCode.CategoryNotFound, $"未找到ID为 '{id}' 的分类"));
            }

            return Ok(ApiResponse<CategoryModel>.Success(category));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取分类时发生错误，ID: {Id}", id);
            return Ok(ApiResponse<CategoryModel>.Fail(ErrorCode.InternalServerError, "获取分类失败"));
        }
    }
    
    [HttpGet("articles/{id}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleModel>>>> GetArticles(string id)
    {
        try
        {
            var articles = await categoryRepository.GetArticlesById(id);
            return Ok(ApiResponse<IEnumerable<ArticleModel>>.Success(articles));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取分类下的文章时发生错误，ID: {Id}", id);
            return Ok(ApiResponse<IEnumerable<ArticleModel>>.Fail(ErrorCode.InternalServerError, "获取分类下的文章失败"));
        }
    }

    /// <summary>
    /// 创建或更新分类（需要管理员身份）
    /// </summary>
    [Authorize(Roles = "Founder, President")]
    [HttpPost("CreateOrUpdate")]
    public async Task<ActionResult<ApiResponse<string>>> CreateOrUpdateCategory([FromBody] CategoryModel category)
    {
        try
        {
            // 数据验证
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(category, new ValidationContext(category), validationResults, true))
            {
                var errorMessage = string.Join(", ", validationResults.Select(v => v.ErrorMessage));
                return Ok(ApiResponse<string>.Fail(ErrorCode.ParameterValidationFailed, errorMessage));
            }

            var result = await categoryRepository.CreateOrUpdate(category);
            if (result)
            {
                return Ok(ApiResponse<string>.Success("分类创建/更新成功"));
            }

            return Ok(ApiResponse<string>.Fail(ErrorCode.OperationFailed, "分类创建/更新失败"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "创建或更新分类时发生错误");
            return Ok(ApiResponse<string>.Fail(ErrorCode.InternalServerError, "创建或更新分类失败"));
        }
    }

    /// <summary>
    /// 删除分类（需要管理员身份）
    /// </summary>
    [Authorize(Roles = "Founder, President")]
    [HttpGet("Delete/{name}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteCategory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Ok(ApiResponse<string>.Fail(ErrorCode.ParameterEmpty, "分类名称不能为空"));
        }

        try
        {
            var result = await categoryRepository.Delete(name);
            if (result)
            {
                return Ok(ApiResponse<string>.Success("分类删除成功"));
            }

            return Ok(ApiResponse<string>.Fail(ErrorCode.CategoryNotFound, "分类不存在"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "删除分类时发生错误，名称: {Name}", name);
            return Ok(ApiResponse<string>.Fail(ErrorCode.InternalServerError, "删除分类失败"));
        }
    }

    /// <summary>
    /// 更新分类顺序（需要管理员身份）
    /// </summary>
    [Authorize(Roles = "Founder, President")]
    [HttpPost("UpdateOrder/{name}/{order:int}")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateCategoryOrder(string name, int order)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Ok(ApiResponse<string>.Fail(ErrorCode.ParameterEmpty, "分类名称不能为空"));
        }

        try
        {
            var result = await categoryRepository.UpdateCategoryOrder(name, order);
            if (result)
            {
                return Ok(ApiResponse<string>.Success("分类顺序更新成功"));
            }

            return Ok(ApiResponse<string>.Fail(ErrorCode.CategoryNotFound, "分类不存在"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新分类顺序时发生错误，名称: {Name}, 顺序: {Order}", name, order);
            return Ok(ApiResponse<string>.Fail(ErrorCode.InternalServerError, "更新分类顺序失败"));
        }
    }

    /// <summary>
    /// 批量更新分类顺序（需要管理员身份）
    /// </summary>
    [Authorize(Roles = "Founder, President")]
    [HttpPost("UpdateOrders")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateCategoryOrders([FromBody] Dictionary<string, int>? categoryOrders)
    {
        if (categoryOrders == null || categoryOrders.Count == 0)
        {
            return Ok(ApiResponse<string>.Fail(ErrorCode.ParameterEmpty, "分类顺序字典不能为空"));
        }

        try
        {
            var result = await categoryRepository.UpdateCategoryOrders(categoryOrders);
            if (result)
            {
                return Ok(ApiResponse<string>.Success("分类顺序批量更新成功"));
            }

            return Ok(ApiResponse<string>.Fail(ErrorCode.OperationFailed, "部分或全部分类更新失败"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "批量更新分类顺序时发生错误");
            return Ok(ApiResponse<string>.Fail(ErrorCode.InternalServerError, "批量更新分类顺序失败"));
        }
    }
}