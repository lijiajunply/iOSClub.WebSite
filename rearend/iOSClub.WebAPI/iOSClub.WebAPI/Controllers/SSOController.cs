using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using iOSClub.DataApi.Services;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
// ReSharper disable once InconsistentNaming
public class SSOController(
    ILoginService loginService,
    IStudentRepository studentRepository,
    IClientApplicationRepository clientAppRepository,
    IConnectionMultiplexer redis,
    IConfiguration config
) : ControllerBase
{
    private readonly IDatabase _redisDb = redis.GetDatabase();

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
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "redirect_uri")] string redirectUri,
        [FromQuery(Name = "state")] string state,
        [FromQuery(Name = "response_type")] string responseType = "code")
    {
        // 验证clientId是否有效
        var clientApp = await clientAppRepository.GetByClientIdAsync(clientId);
        if (clientApp is not { IsActive: true })
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

        // 存储OAuth2参数到会话中
        HttpContext.Session.SetString("OAuthState", encryptedState);
        HttpContext.Session.SetString("OAuthClientId", clientId);
        HttpContext.Session.SetString("OAuthRedirectUri", redirectUri);
        HttpContext.Session.SetString("OAuthResponseType", responseType);

        var clientAppUrl = Environment.GetEnvironmentVariable("CLIENTAPPURL", EnvironmentVariableTarget.Process);

        if (string.IsNullOrEmpty(clientAppUrl))
        {
            clientAppUrl = config["ClientAppUrl"] ?? "http://localhost:5173";
        }

        // 重定向到我们自己的OAuth登录页面
        return Redirect(
            $"{clientAppUrl}/oauth-login?state={encryptedState}&client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type={responseType}");
    }

    /// <summary>
    /// OAuth提供商回调处理
    /// </summary>
    /// <param name="state">加密的状态信息</param>
    /// <returns>重定向到第三方应用</returns>
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string state)
    {
        // 从Redis中获取用户信息（这应该在OAuthLogin页面成功登录后设置）
        var userId = HttpContext.Session.GetString("OAuthAuthenticatedUserId");
        var token = HttpContext.Session.GetString("OAuthAuthenticatedToken");

        // 如果会话中没有找到用户信息，尝试从Redis中获取
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            // 解密state参数以获取原始state值作为Redis键
            try
            {
                var decryptedState = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(state));
                var stateInfo = System.Text.Json.JsonSerializer.Deserialize<AuthState>(decryptedState) ??
                                throw new InvalidOperationException();

                // 使用state作为key从Redis中获取用户信息
                var redisKey = $"oauth:auth:{stateInfo.State}";
                var userInfoJson = await _redisDb.StringGetAsync(redisKey);

                if (!userInfoJson.IsNullOrEmpty)
                {
                    var userInfo = System.Text.Json.JsonSerializer.Deserialize<OAuthUserInfo>(userInfoJson.ToString());
                    if (userInfo != null)
                    {
                        userId = userInfo.UserId;
                        token = userInfo.Token;

                        // 删除Redis中的临时数据
                        await _redisDb.KeyDeleteAsync(redisKey);
                    }
                }
            }
            catch
            {
                // 解密或反序列化失败，保持userId和token为null或empty
            }
        }

        // 检查是否成功获取到用户信息
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            return BadRequest("用户认证失败或会话已过期");

        // 清除会话中的用户信息
        HttpContext.Session.Remove("OAuthAuthenticatedUserId");
        HttpContext.Session.Remove("OAuthAuthenticatedUserName");
        HttpContext.Session.Remove("OAuthAuthenticatedToken");
        HttpContext.Session.Remove("TempOAuthToken");

        // 首先尝试从Session中获取OAuth参数
        var authState = new AuthState();
        var sessionOAuthState = HttpContext.Session.GetString("OAuthState");
        var sessionClientId = HttpContext.Session.GetString("OAuthClientId");
        var sessionRedirectUri = HttpContext.Session.GetString("OAuthRedirectUri");
        var sessionResponseType = HttpContext.Session.GetString("OAuthResponseType");

        if (!string.IsNullOrEmpty(sessionOAuthState) &&
            !string.IsNullOrEmpty(sessionClientId) &&
            !string.IsNullOrEmpty(sessionRedirectUri) &&
            !string.IsNullOrEmpty(sessionResponseType))
        {
            // 从Session中获取OAuth参数
            authState.ClientId = sessionClientId;
            authState.RedirectUri = sessionRedirectUri;
            authState.ResponseType = sessionResponseType;

            // 从state参数中解析State值
            try
            {
                var decryptedState = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(state));
                var stateInfo = System.Text.Json.JsonSerializer.Deserialize<AuthState>(decryptedState) ??
                                throw new InvalidOperationException();
                authState.State = stateInfo.State;
            }
            catch
            {
                return BadRequest("无效的状态参数");
            }
        }
        else
        {
            // Session中没有OAuth参数，从state参数中解析所有信息
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
        }

        // 验证token是否有效
        var isValid = await ValidateToken(token);
        if (!isValid)
            return BadRequest("无效的认证令牌");

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
        // 添加参数验证
        if (string.IsNullOrEmpty(grantType))
            return BadRequest("缺少必需参数: grant_type");

        if (grantType != "authorization_code")
            return BadRequest("不支持的授权类型: " + grantType);

        if (string.IsNullOrEmpty(code))
            return BadRequest("缺少必需参数: code");

        if (string.IsNullOrEmpty(clientId))
            return BadRequest("缺少必需参数: client_id");

        if (string.IsNullOrEmpty(clientSecret))
            return BadRequest("缺少必需参数: client_secret");

        if (string.IsNullOrEmpty(redirectUri))
            return BadRequest("缺少必需参数: redirect_uri");

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
        catch (FormatException)
        {
            return BadRequest("无效的授权码格式");
        }
        catch (Exception ex)
        {
            return BadRequest($"处理请求时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 验证访问令牌
    /// </summary>
    /// <param name="token">要验证的令牌</param>
    /// <returns>令牌是否有效</returns>
    private async Task<bool> ValidateToken(string token)
    {
        // 检查令牌是否为空
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            // 使用JwtHelper中的配置来验证令牌
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            // 检查令牌是否过期
            if (jsonToken.ValidTo < DateTime.UtcNow)
                return false;

            // 从令牌中提取用户ID
            var userId = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return false;

            // 使用loginService来验证令牌
            return await loginService.ValidateToken(userId, token);
        }
        catch (Exception)
        {
            // 如果解析令牌时出现任何异常，认为令牌无效
            return false;
        }
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <returns>用户信息</returns>
    [HttpGet("userinfo")]
    [Authorize]
    public async Task<IActionResult> UserInfo()
    {
        var user = HttpContext.User.GetUser();
        if (user == null)
            return Unauthorized();

        var member = await studentRepository.GetByIdAsync(user.UserId);

        if (member == null)
            return Unauthorized();

        return Ok(new
        {
            sub = user.UserId,
            name = member.EMail,
            email = member.EMail,
            role = user.Identity
        });
    }

    /// <summary>
    /// 存储用户认证信息到会话（用于OAuth登录流程）
    /// </summary>
    /// <param name="request">请求参数</param>
    /// <returns>操作结果</returns>
    [HttpPost("store-session")]
    [Authorize]
    public async Task<IActionResult> StoreSession([FromBody] StoreSessionRequest request)
    {
        try
        {
            // 获取当前认证的用户信息
            var user = HttpContext.User.GetUser();
            if (user == null)
            {
                return Unauthorized("用户未认证");
            }

            // 从请求头获取token
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var token = authHeader.StartsWith("Bearer ") ? authHeader["Bearer ".Length..].Trim() : authHeader;

            // 存储用户信息到会话中，供callback方法使用
            HttpContext.Session.SetString("OAuthAuthenticatedUserId", user.UserId);
            HttpContext.Session.SetString("OAuthAuthenticatedUserName", user.UserName);
            HttpContext.Session.SetString("OAuthAuthenticatedToken", token);

            // 同时存储到Redis中，使用state作为key
            if (string.IsNullOrEmpty(request.State)) return Ok(new { success = true, message = "会话存储成功" });
            try
            {
                // 解密state参数以获取原始state值作为Redis键
                var decryptedState = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(request.State));
                var authState = System.Text.Json.JsonSerializer.Deserialize<AuthState>(decryptedState) ??
                                throw new InvalidOperationException();

                var redisKey = $"oauth:auth:{authState.State}";
                var userInfo = new OAuthUserInfo
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Token = token
                };

                // 存储到Redis，设置5分钟过期时间
                await _redisDb.StringSetAsync(
                    redisKey,
                    System.Text.Json.JsonSerializer.Serialize(userInfo),
                    TimeSpan.FromMinutes(5));
            }
            catch
            {
                // 如果Redis存储失败，仅记录日志（但在此代码中我们不记录日志）
                // 继续执行，因为会话存储已经成功
            }

            return Ok(new { success = true, message = "会话存储成功" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 存储会话请求模型
    /// </summary>
    public class StoreSessionRequest
    {
        [JsonProperty("state")] public string State { get; set; } = "";
        [JsonProperty("client_id")] public string ClientId { get; set; } = "";
    }

    /// <summary>
    /// 获取客户端应用信息
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <returns>客户端应用信息</returns>
    [HttpGet("client-info")]
    public async Task<IActionResult> GetClientInfo(string clientId)
    {
        if (string.IsNullOrEmpty(clientId))
            return BadRequest("无效的客户端ID");

        var clientApp = await clientAppRepository.GetByClientIdAsync(clientId);
        if (clientApp is not { IsActive: true })
            return NotFound("客户端应用不存在或已禁用");

        return Ok(new
        {
            name = clientApp.ApplicationName,
            description = clientApp.Description,
            client_id = clientApp.ClientId
        });
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

    /// <summary>
    /// OAuth用户信息
    /// </summary>
    public class OAuthUserInfo
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Token { get; set; } = "";
    }
}