using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.Common;
using System.Security.Cryptography;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Founder, President, Minister")]
public class ClientAppController(IClientApplicationRepository clientAppRepository) : ControllerBase
{
    /// <summary>
    /// 获取所有客户端应用
    /// </summary>
    /// <returns>客户端应用列表</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClientApplication>>>> GetClientApplications()
    {
        try
        {
            var clientApps = await clientAppRepository.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ClientApplication>>.Success(clientApps));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<IEnumerable<ClientApplication>>.Fail(ErrorCode.InternalServerError, "获取客户端应用列表失败"));
        }
    }

    /// <summary>
    /// 根据客户端ID获取应用
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <returns>客户端应用</returns>
    [HttpGet("{clientId}")]
    public async Task<ActionResult<ApiResponse<ClientApplication>>> GetClientApplication(string clientId)
    {
        try
        {
            var clientApp = await clientAppRepository.GetByClientIdAsync(clientId);
            if (clientApp == null)
                return Ok(ApiResponse<ClientApplication>.Fail(ErrorCode.ResourceNotFound, "客户端应用不存在"));

            return Ok(ApiResponse<ClientApplication>.Success(clientApp));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<ClientApplication>.Fail(ErrorCode.InternalServerError, "获取客户端应用失败"));
        }
    }

    /// <summary>
    /// 创建新的客户端应用
    /// </summary>
    /// <param name="clientAppModel">客户端应用信息</param>
    /// <returns>创建的客户端应用</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ClientAppResultModel>>> CreateClientApplication(CreateClientAppModel clientAppModel)
    {
        try
        {
            // 生成客户端ID和密钥
            var clientId = GenerateClientId();
            var clientSecret = GenerateClientSecret();

            var clientApp = new ClientApplication
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                ApplicationName = clientAppModel.ApplicationName,
                Description = clientAppModel.Description,
                HomepageUrl = clientAppModel.HomepageUrl,
                RedirectUris = string.Join(";", clientAppModel.RedirectUris),
                LogoUrl = clientAppModel.LogoUrl,
                IsActive = true,
                IsNeedEMail = clientAppModel.IsNeedEMail,
                SupportsPkce = clientAppModel.SupportsPkce
            };

            // 保存原始密钥用于返回给客户端
            var originalClientSecret = clientApp.ClientSecret;

            var result = await clientAppRepository.CreateAsync(clientApp);
            if (!result)
                return Ok(ApiResponse<ClientAppResultModel>.Fail(ErrorCode.OperationFailed, "创建客户端应用失败"));

            // 返回包含密钥的信息（只在创建时显示）
            var resultModel = new ClientAppResultModel
            {
                ClientId = clientApp.ClientId,
                // 返回原始密钥而不是哈希值
                ClientSecret = originalClientSecret,
                ApplicationName = clientApp.ApplicationName,
                Description = clientApp.Description,
                HomepageUrl = clientApp.HomepageUrl,
                RedirectUris = clientApp.RedirectUris.Split(';').ToList(),
                LogoUrl = clientApp.LogoUrl,
                IsActive = clientApp.IsActive,
                IsNeedEMail = clientApp.IsNeedEMail,
                SupportsPkce = clientApp.SupportsPkce
            };

            return CreatedAtAction(nameof(GetClientApplication), new { clientId = clientApp.ClientId }, ApiResponse<ClientAppResultModel>.Success(resultModel, "创建客户端应用成功"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<ClientAppResultModel>.Fail(ErrorCode.InternalServerError, "创建客户端应用失败"));
        }
    }

    /// <summary>
    /// 更新客户端应用
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="clientAppModel">客户端应用信息</param>
    /// <returns>更新结果</returns>
    [HttpPut("{clientId}")]
    public async Task<ActionResult<ApiResponse>> UpdateClientApplication(string clientId, UpdateClientAppModel clientAppModel)
    {
        try
        {
            var existingClientApp = await clientAppRepository.GetByClientIdAsync(clientId);
            if (existingClientApp == null)
                return Ok(ApiResponse.Fail(ErrorCode.ResourceNotFound, "客户端应用不存在"));

            existingClientApp.ApplicationName = clientAppModel.ApplicationName;
            existingClientApp.Description = clientAppModel.Description;
            existingClientApp.HomepageUrl = clientAppModel.HomepageUrl;
            existingClientApp.RedirectUris = string.Join(";", clientAppModel.RedirectUris);
            existingClientApp.LogoUrl = clientAppModel.LogoUrl;
            existingClientApp.IsActive = clientAppModel.IsActive;
            existingClientApp.UpdatedAt = DateTime.UtcNow;
            existingClientApp.IsNeedEMail = clientAppModel.IsNeedEMail;
            existingClientApp.SupportsPkce = clientAppModel.SupportsPkce;

            var result = await clientAppRepository.UpdateAsync(existingClientApp);
            if (!result)
                return Ok(ApiResponse.Fail(ErrorCode.OperationFailed, "更新客户端应用失败"));

            return Ok(ApiResponse.Success("更新客户端应用成功"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse.Fail(ErrorCode.InternalServerError, "更新客户端应用失败"));
        }
    }

    /// <summary>
    /// 删除客户端应用
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{clientId}")]
    public async Task<ActionResult<ApiResponse>> DeleteClientApplication(string clientId)
    {
        try
        {
            var result = await clientAppRepository.DeleteAsync(clientId);
            if (!result)
                return Ok(ApiResponse.Fail(ErrorCode.ResourceNotFound, "客户端应用不存在"));

            return Ok(ApiResponse.Success("删除客户端应用成功"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse.Fail(ErrorCode.InternalServerError, "删除客户端应用失败"));
        }
    }

    /// <summary>
    /// 重新生成客户端密钥
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <returns>新的客户端密钥</returns>
    [HttpPost("{clientId}/regenerate-secret")]
    public async Task<ActionResult<ApiResponse<RegenerateSecretResult>>> RegenerateClientSecret(string clientId)
    {
        try
        {
            var existingClientApp = await clientAppRepository.GetByClientIdAsync(clientId);
            if (existingClientApp == null)
                return Ok(ApiResponse<RegenerateSecretResult>.Fail(ErrorCode.ResourceNotFound, "客户端应用不存在"));

            var newSecret = GenerateClientSecret();
            // 保存原始密钥用于返回给客户端，但存储到数据库的是哈希值
            existingClientApp.ClientSecret = newSecret;
            existingClientApp.UpdatedAt = DateTime.UtcNow;

            var result = await clientAppRepository.UpdateAsync(existingClientApp);
            if (!result)
                return Ok(ApiResponse<RegenerateSecretResult>.Fail(ErrorCode.OperationFailed, "重新生成密钥失败"));

            var resultModel = new RegenerateSecretResult { ClientId = clientId, NewSecret = newSecret };
            return Ok(ApiResponse<RegenerateSecretResult>.Success(resultModel, "重新生成密钥成功"));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<RegenerateSecretResult>.Fail(ErrorCode.InternalServerError, "重新生成密钥失败"));
        }
    }

    /// <summary>
    /// 生成客户端ID
    /// </summary>
    /// <returns>客户端ID</returns>
    private static string GenerateClientId()
    {
        return "ca_" + Guid.NewGuid().ToString("N")[..16];
    }

    /// <summary>
    /// 生成客户端密钥
    /// </summary>
    /// <returns>客户端密钥</returns>
    private static string GenerateClientSecret()
    {
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        return Convert.ToBase64String(bytes);
    }
}

/// <summary>
/// 创建客户端应用模型
/// </summary>
[Serializable]
public class CreateClientAppModel
{
    /// <summary>
    /// 应用名称
    /// </summary>
    public string ApplicationName { get; set; } = "";

    /// <summary>
    /// 应用描述
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 应用主页URL
    /// </summary>
    public string HomepageUrl { get; set; } = "";

    /// <summary>
    /// 回调URL列表
    /// </summary>
    public List<string> RedirectUris { get; set; } = [];

    /// <summary>
    /// 应用图标URL
    /// </summary>
    public string LogoUrl { get; set; } = "";

    /// <summary>
    /// 是否需要邮箱验证
    /// </summary>
    public bool IsNeedEMail { get; set; }

    /// <summary>
    /// 是否支持PKCE
    /// </summary>
    public bool SupportsPkce { get; set; }
}

/// <summary>
/// 更新客户端应用模型
/// </summary>
[Serializable]
public class UpdateClientAppModel
{
    /// <summary>
    /// 应用名称
    /// </summary>
    public string ApplicationName { get; set; } = "";

    /// <summary>
    /// 应用描述
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 应用主页URL
    /// </summary>
    public string HomepageUrl { get; set; } = "";

    /// <summary>
    /// 回调URL列表
    /// </summary>
    public List<string> RedirectUris { get; set; } = [];

    /// <summary>
    /// 应用图标URL
    /// </summary>
    public string LogoUrl { get; set; } = "";

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 是否需要邮箱验证
    /// </summary>
    public bool IsNeedEMail { get; set; }
    
    /// <summary>
    /// 是否支持PKCE
    /// </summary>
    public bool SupportsPkce { get; set; }
}

/// <summary>
/// 客户端应用结果模型（包含密钥）
/// </summary>
[Serializable]
public class ClientAppResultModel
{
    /// <summary>
    /// 客户端ID
    /// </summary>
    public string ClientId { get; set; } = "";

    /// <summary>
    /// 客户端密钥
    /// </summary>
    public string ClientSecret { get; set; } = "";

    /// <summary>
    /// 应用名称
    /// </summary>
    public string ApplicationName { get; set; } = "";

    /// <summary>
    /// 应用描述
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 应用主页URL
    /// </summary>
    public string HomepageUrl { get; set; } = "";

    /// <summary>
    /// 回调URL列表
    /// </summary>
    public List<string> RedirectUris { get; set; } = [];

    /// <summary>
    /// 应用图标URL
    /// </summary>
    public string LogoUrl { get; set; } = "";

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 是否需要邮箱验证
    /// </summary>
    public bool IsNeedEMail { get; set; }

    /// <summary>
    /// 是否支持PKCE
    /// </summary>
    public bool SupportsPkce { get; set; }
}

/// <summary>
/// 重新生成密钥结果
/// </summary>
[Serializable]
public class RegenerateSecretResult
{
    /// <summary>
    /// 客户端ID
    /// </summary>
    public string ClientId { get; set; } = "";

    /// <summary>
    /// 新的客户端密钥
    /// </summary>
    public string NewSecret { get; set; } = "";
}