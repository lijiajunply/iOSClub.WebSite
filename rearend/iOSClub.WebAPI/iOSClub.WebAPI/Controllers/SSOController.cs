using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using iOSClub.DataApi.Services;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
// ReSharper disable once InconsistentNaming
public class SSOController(
    ILoginService loginService,
    IClientApplicationRepository clientAppRepository) : ControllerBase
{
    /// <summary>
    /// 为第三方应用提供OAuth登录入口
    /// </summary>
    /// <param name="clientId">第三方应用的客户端ID</param>
    /// <param name="redirectUri">第三方应用的回调地址</param>
    /// <param name="state">用于防止CSRF攻击的随机字符串</param>
    /// <param name="responseType">响应类型，支持code或token</param>
    /// <returns>重定向到OAuth提供商</returns>
    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize(
        [FromQuery] string clientId,
        [FromQuery] string redirectUri,
        [FromQuery] string state,
        [FromQuery] string responseType = "code")
    {
        // 验证clientId是否有效
        var clientApp = await clientAppRepository.GetByClientIdAsync(clientId);
        if (clientApp == null || !clientApp.IsActive)
            return BadRequest("无效的客户端ID");

        // 验证redirectUri是否在白名单中
        if (!clientApp.IsRedirectUriValid(redirectUri))
            return BadRequest("无效的回调地址");

        // 将第三方应用的信息存储在state中，以便在回调时使用
        var authState = new AuthState
        {
            ClientId = clientId,
            RedirectUri = redirectUri,
            State = state,
            ResponseType = responseType
        };

        // 将authState序列化并加密，然后作为state参数传递
        var encryptedState = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            System.Text.Json.JsonSerializer.Serialize(authState)));

        // 重定向到OAuth提供商
        var provider = "OAuth2";
        var redirectUrl = Url.Action(nameof(Callback), "SSO", new { state = encryptedState });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }

    /// <summary>
    /// OAuth提供商回调处理
    /// </summary>
    /// <param name="state">加密的状态信息</param>
    /// <returns>重定向到第三方应用</returns>
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string state)
    {
        var authenticateResult = await HttpContext.AuthenticateAsync("OAuth2");

        if (!authenticateResult.Succeeded)
            return BadRequest("OAuth2 认证失败");

        var claims = authenticateResult.Principal?.Claims;
        if (claims == null)
            return BadRequest("无法获取用户信息");

        // 获取用户标识信息
        var enumerable = claims as Claim[] ?? claims.ToArray();
        var userId = enumerable.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var userName = enumerable.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var email = enumerable.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(userId))
            return BadRequest("无法获取用户ID");

        // 解密state参数
        AuthState authState;
        try
        {
            var decryptedState = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(state));
            authState = System.Text.Json.JsonSerializer.Deserialize<AuthState>(decryptedState) ??
                        throw new InvalidOperationException();
        }
        catch
        {
            return BadRequest("无效的状态参数");
        }

        // 使用 loginService 进行用户登录并生成 Token
        var token = await loginService.StaffLogin(userId, userName ?? userId);

        // 如果员工登录失败，尝试普通用户登录
        if (string.IsNullOrEmpty(token))
        {
            token = await loginService.Login(userId, "defaultPassword");
        }

        // 如果登录仍然失败，返回错误
        if (string.IsNullOrEmpty(token))
            return BadRequest("用户登录失败");

        // 根据responseType决定返回方式
        if (authState.ResponseType == "code")
        {
            // 生成授权码（简化实现，实际应该有更复杂的逻辑）
            var authCode = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token));

            // 重定向到第三方应用的回调地址
            var redirectUrl = $"{authState.RedirectUri}?code={authCode}&state={authState.State}";
            return Redirect(redirectUrl);
        }

        if (authState.ResponseType == "token")
        {
            // 直接返回访问令牌
            var redirectUrl = $"{authState.RedirectUri}#access_token={token}&state={authState.State}";
            return Redirect(redirectUrl);
        }

        return BadRequest("不支持的响应类型");
    }

    /// <summary>
    /// 第三方应用通过授权码获取访问令牌
    /// </summary>
    /// <param name="grantType">授权类型，支持authorization_code</param>
    /// <param name="code">授权码</param>
    /// <param name="clientId">客户端ID</param>
    /// <param name="clientSecret">客户端密钥</param>
    /// <param name="redirectUri">重定向URI</param>
    /// <returns>访问令牌</returns>
    [HttpPost("token")]
    public async Task<IActionResult> Token(
        [FromForm] string grantType,
        [FromForm] string code,
        [FromForm] string clientId,
        [FromForm] string clientSecret,
        [FromForm] string redirectUri)
    {
        if (grantType != "authorization_code")
            return BadRequest("不支持的授权类型");

        // 验证客户端凭据
        var clientApp = await clientAppRepository.ValidateCredentialsAsync(clientId, clientSecret);
        if (clientApp == null)
            return Unauthorized("无效的客户端凭据");

        // 验证回调URL是否匹配
        if (!clientApp.IsRedirectUriValid(redirectUri))
            return BadRequest("无效的回调地址");

        try
        {
            // 解码授权码获取令牌（简化实现）
            var token = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(code));

            // 验证令牌是否有效
            var isValid = await ValidateToken(token);
            if (!isValid)
                return BadRequest("无效的授权码");

            return Ok(new
            {
                access_token = token,
                token_type = "Bearer",
                expires_in = 7200 // 2小时
            });
        }
        catch
        {
            return BadRequest("无效的授权码");
        }
    }

    /// <summary>
    /// 验证访问令牌
    /// </summary>
    /// <param name="token">要验证的令牌</param>
    /// <returns>令牌是否有效</returns>
    private async Task<bool> ValidateToken(string token)
    {
        // 在实际应用中，这里应该有更复杂的令牌验证逻辑
        // 包括检查令牌是否过期、是否被撤销等
        return !string.IsNullOrEmpty(token);
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <returns>用户信息</returns>
    [HttpGet("userinfo")]
    [Authorize]
    public IActionResult UserInfo()
    {
        var user = HttpContext.User.GetUser();
        if (user == null)
            return Unauthorized();

        return Ok(new
        {
            sub = user.UserId,
            name = user.UserName,
            email = user.EMail,
            role = user.Identity
        });
    }
}

/// <summary>
/// 授权状态信息
/// </summary>
public class AuthState
{
    public string ClientId { get; set; } = "";
    public string RedirectUri { get; set; } = "";
    public string State { get; set; } = "";
    public string ResponseType { get; set; } = "";
}