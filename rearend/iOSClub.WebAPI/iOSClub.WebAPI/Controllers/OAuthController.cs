using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using iOSClub.DataApi.Services;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class OAuthController(ILoginService loginService, IConfiguration configuration) : ControllerBase
{
    [HttpGet("login")]
    public IActionResult Login(string provider = "OAuth2", string? returnUrl = "/")
    {
        // 检查是否配置了OAuth
        var clientId = configuration["OAuth2:ClientId"];
        if (string.IsNullOrEmpty(clientId))
        {
            return BadRequest("OAuth2 未正确配置");
        }

        var redirectUrl = Url.Action(nameof(Callback), "OAuth", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string? returnUrl = "/")
    {
        // 检查是否配置了OAuth
        var clientId = configuration["OAuth2:ClientId"];
        if (string.IsNullOrEmpty(clientId))
        {
            return BadRequest("OAuth2 未正确配置");
        }

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

        if (string.IsNullOrEmpty(userId))
            return BadRequest("无法获取用户ID");

        // 使用 loginService 进行用户登录并生成 Token
        // 首先尝试员工登录
        var token = await loginService.StaffLogin(userId, userName ?? userId);

        // 如果员工登录失败，尝试普通用户登录
        // 注意：这里需要一个默认密码，实际应用中应该有更安全的方式
        if (string.IsNullOrEmpty(token))
        {
            // 这里使用一个默认密码，实际应用中应该从安全的地方获取
            token = await loginService.Login(userId, "defaultPassword");
        }

        // 如果登录仍然失败，返回错误
        if (string.IsNullOrEmpty(token))
            return BadRequest("用户登录失败");

        // 返回 token 给客户端
        return Ok(new { token, returnUrl });
    }

    [HttpPost("exchange")]
    public async Task<IActionResult> ExchangeToken([FromBody] OAuthExchangeRequest request)
    {
        // 如果你有一个外部 OAuth 提供商的访问令牌，
        // 可以在这里验证并交换为内部 JWT Token

        // 这是一个示例，实际实现会根据你的 OAuth 提供商而有所不同
        // 通常你需要调用 OAuth 提供商的 API 来验证访问令牌

        // 使用 loginService 验证用户并生成 JWT Token
        if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.UserName))
            return BadRequest("用户ID和用户名不能为空");

        // 首先尝试员工登录
        var token = await loginService.StaffLogin(request.UserId, request.UserName);

        // 如果员工登录失败，尝试普通用户登录
        if (string.IsNullOrEmpty(token))
        {
            // 这里使用一个默认密码，实际应用中应该从安全的地方获取
            token = await loginService.Login(request.UserId, "defaultPassword");
        }

        // 如果登录仍然失败，返回错误
        if (string.IsNullOrEmpty(token))
            return BadRequest("用户登录失败");

        return Ok(new { Token = token });
    }
}

public class OAuthExchangeRequest
{
    public string? AccessToken { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
}