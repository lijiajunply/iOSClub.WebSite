using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using iOSClub.DataApi.Services;
using iOSClub.DataApi.Repositories;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
// ReSharper disable once InconsistentNaming
public class SSOController(
    ILoginService loginService,
    IStudentRepository studentRepository,
    IStaffRepository staffRepository,
    IClientApplicationRepository clientAppRepository,
    IConnectionMultiplexer redis,
    IConfiguration config,
    ILogger<SSOController> logger)
    : ControllerBase
{
    private readonly IDatabase _redisDb = redis.GetDatabase();
    private const string DefaultScore = "profile openid role";

    private const string KeyFilePath = "rsa-key.xml";
    private static RsaSecurityKey? _rsaKey;

    /// <summary>
    /// 获取或创建 RSA 密钥
    /// </summary>
    private static RsaSecurityKey GetRsaKey()
    {
        if (_rsaKey != null) return _rsaKey;

        var rsa = RSA.Create(2048);

        if (System.IO.File.Exists(KeyFilePath))
        {
            // 如果文件存在，读取旧密钥（保持重启后 Token 有效）
            var xmlString = System.IO.File.ReadAllText(KeyFilePath);
            rsa.FromXmlString(xmlString);
        }
        else
        {
            // 如果文件不存在，生成新密钥并保存
            var xmlString = rsa.ToXmlString(true); // true 表示包含私钥
            System.IO.File.WriteAllText(KeyFilePath, xmlString);
        }

        _rsaKey = new RsaSecurityKey(rsa)
        {
            KeyId = "main-key-1" // 给密钥一个 ID，便于轮换
        };

        return _rsaKey;
    }

    /// <summary>
    /// OpenID Connect discovery document endpoint
    /// </summary>
    /// <returns>Discovery document</returns>
    [HttpGet("/.well-known/openid-configuration")]
    public IActionResult GetDiscoveryDocument()
    {
        const string issuer = "https://api.xauat.site";

        var discoveryDoc = new
        {
            issuer,
            authorization_endpoint = $"{issuer}/SSO/authorize",
            token_endpoint = $"{issuer}/SSO/token",
            userinfo_endpoint = $"{issuer}/SSO/userinfo",
            jwks_uri = $"{issuer}/SSO/jwks",
            response_types_supported = new[] { "code", "token", "id_token", "id_token token" },
            subject_types_supported = new[] { "public" },
            id_token_signing_alg_values_supported = new[] { "RS256" }, // Changed from HS256 to RS256
            scopes_supported = new[] { "openid", "profile", "email", "read", "phone" },
            token_endpoint_auth_methods_supported = new[] { "client_secret_post" },
            claims_supported = new[]
                { "sub", "name", "nickname", "email", "role", "phone", "academy", "class", "joinTime", "avatar" }
        };

        return Ok(discoveryDoc);
    }

    /// <summary>
    /// JWKS endpoint for validating ID tokens
    /// </summary>
    /// <returns>JWKS</returns>
    [HttpGet("jwks")]
    [AllowAnonymous]
    public IActionResult GetJwks()
    {
        // Get the RSA public key parameters
        var rsaKey = GetRsaKey();
        var rsaParameters =
            (rsaKey.Rsa ?? throw new InvalidOperationException("RSA key not found"))
            .ExportParameters(false); // false = 只导出公钥

        logger.LogInformation("JWKS request received");

        if (rsaParameters.Modulus == null || rsaParameters.Exponent == null)
        {
            logger.LogError("Failed to retrieve RSA public key parameters");
            return StatusCode(500, "Failed to retrieve RSA public key parameters");
        }

        // Convert RSA public key parameters to JWKS format
        var modulus = Convert.ToBase64String(rsaParameters.Modulus, Base64FormattingOptions.None)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        var exponent = Convert.ToBase64String(rsaParameters.Exponent, Base64FormattingOptions.None)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        var jwks = new
        {
            keys = new[]
            {
                new
                {
                    kty = "RSA", // RSA key type
                    alg = "RS256", // RSA with SHA-256
                    kid = rsaKey.KeyId ?? "main-key-1", // Use the key ID from the RsaSecurityKey
                    use = "sig", // Signature key
                    n = modulus, // Modulus
                    e = exponent // Exponent
                }
            }
        };

        return Ok(jwks);
    }

    /// <summary>
    /// 为第三方应用提供OAuth登录入口
    /// </summary>
    /// <param name="clientId">第三方应用的客户端ID</param>
    /// <param name="redirectUri">第三方应用的回调地址</param>
    /// <param name="state">用于防止CSRF攻击的随机字符串</param>
    /// <param name="responseType">响应类型，支持code、token、id_token</param>
    /// <param name="codeChallenge">PKCE代码挑战</param>
    /// <param name="codeChallengeMethod">PKCE代码挑战方法</param>
    /// <param name="scope">请求的权限范围</param>
    /// <param name="nonce">用于防止重放攻击的随机字符串</param>
    /// <returns>重定向到OAuth提供商</returns>
    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize(
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "redirect_uri")] string redirectUri,
        [FromQuery(Name = "state")] string state,
        [FromQuery(Name = "response_type")] string responseType = "code",
        [FromQuery(Name = "code_challenge")] string? codeChallenge = null,
        [FromQuery(Name = "code_challenge_method")]
        string? codeChallengeMethod = null,
        [FromQuery(Name = "scope")] string? scope = null,
        [FromQuery(Name = "nonce")] string? nonce = null)
    {
        logger.LogInformation("OAuth authorization request received for client {ClientId}", clientId);

        // 验证clientId是否有效
        var clientApp = await clientAppRepository.GetByClientIdAsync(clientId);
        if (clientApp is not { IsActive: true })
        {
            logger.LogWarning("Authorization failed: invalid client ID {ClientId}", clientId);
            return BadRequest("无效的客户端ID");
        }

        // 验证redirectUri是否在白名单中
        if (!clientApp.IsRedirectUriValid(redirectUri))
        {
            logger.LogWarning("Authorization failed: invalid redirect URI {RedirectUri} for client {ClientId}",
                redirectUri, clientId);
            return BadRequest("无效的回调地址");
        }

        // 如果客户端支持PKCE，则要求提供code_challenge参数
        if (clientApp.SupportsPkce && string.IsNullOrEmpty(codeChallenge))
        {
            logger.LogWarning(
                "Authorization failed: PKCE is required for client {ClientId} but code_challenge is missing", clientId);
            return BadRequest("客户端要求使用PKCE，必须提供code_challenge参数");
        }

        // 如果提供了code_challenge，验证其格式
        if (!string.IsNullOrEmpty(codeChallenge))
        {
            // 验证code_challenge_method
            if (string.IsNullOrEmpty(codeChallengeMethod))
            {
                codeChallengeMethod = "S256"; // 默认为S256
            }

            if (codeChallengeMethod != "S256" && codeChallengeMethod != "plain")
            {
                logger.LogWarning(
                    "Authorization failed: unsupported code_challenge_method {Method} for client {ClientId}",
                    codeChallengeMethod, clientId);
                return BadRequest("不支持的code_challenge_method");
            }

            // 验证code_challenge长度
            if (codeChallenge.Length is < 43 or > 128)
            {
                logger.LogWarning("Authorization failed: invalid code_challenge length for client {ClientId}",
                    clientId);
                return BadRequest("code_challenge长度无效");
            }
        }

        // 验证scope参数
        var validScopes = new[] { "openid", "profile", "email", "read", "phone" };
        var requestedScopes = (scope ?? DefaultScore).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var validRequestedScopes = requestedScopes.Where(s => validScopes.Contains(s)).ToList();

        // 确保始终包含openid scope
        if (!validRequestedScopes.Contains("openid") && (responseType.Contains("id_token") || responseType == "token"))
        {
            validRequestedScopes.Add("openid");
        }

        var finalScope = string.Join(" ", validRequestedScopes);

        // 将第三方应用的信息存储在state中，以便在回调时使用
        var authState = new AuthState
        {
            ClientId = clientId,
            RedirectUri = redirectUri,
            State = state,
            ResponseType = responseType,
            CodeChallenge = codeChallenge ?? "",
            CodeChallengeMethod = codeChallengeMethod ?? "",
            Scope = finalScope,
            Nonce = nonce ?? ""
        };

        // 将authState序列化并加密，然后作为state参数传递
        var encryptedState = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            System.Text.Json.JsonSerializer.Serialize(authState)));

        // 存储OAuth2参数到Redis中
        var oauthParamsKey = $"oauth:params:{authState.State}";
        await _redisDb.StringSetAsync(oauthParamsKey, System.Text.Json.JsonSerializer.Serialize(authState),
            TimeSpan.FromMinutes(10));

        var clientAppUrl = Environment.GetEnvironmentVariable("CLIENTAPPURL", EnvironmentVariableTarget.Process);

        if (string.IsNullOrEmpty(clientAppUrl))
        {
            clientAppUrl = config["ClientAppUrl"] ?? "http://localhost:5173";
        }

        logger.LogInformation("Redirecting to OAuth login page for client {ClientId}", clientId);

        // 重定向到我们自己的OAuth登录页面
        return Redirect(
            $"{clientAppUrl}/oauth-login?state={encryptedState}&client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type={responseType}&scope={Uri.EscapeDataString(authState.Scope)}");
    }

    /// <summary>
    /// OAuth提供商回调处理
    /// </summary>
    /// <param name="state">加密的状态信息</param>
    /// <returns>重定向到第三方应用</returns>
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string state)
    {
        logger.LogInformation("OAuth callback received with state parameter");

        // 从Redis中获取用户信息（这应该在OAuthLogin页面成功登录后设置）
        var userId = "";
        var token = "";

        // 解密state参数以获取原始state值作为Redis键
        try
        {
            var decryptedState = Encoding.UTF8.GetString(Convert.FromBase64String(state));
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to decrypt or deserialize state parameter");
            // 解密或反序列化失败，保持userId和token为null或empty
        }

        // 检查是否成功获取到用户信息
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            logger.LogWarning("Callback failed: user authentication failed or session expired");
            return BadRequest("用户认证失败或会话已过期");
        }

        // 从state参数中解析所有信息
        AuthState authState;
        try
        {
            var decryptedState = Encoding.UTF8.GetString(Convert.FromBase64String(state));
            authState = System.Text.Json.JsonSerializer.Deserialize<AuthState>(decryptedState) ??
                        throw new InvalidOperationException();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to decrypt or deserialize state parameter");
            return BadRequest("无效的状态参数");
        }

        // 从Redis中获取OAuth参数
        var oauthParamsKey = $"oauth:params:{authState.State}";
        var oauthParamsJson = await _redisDb.StringGetAsync(oauthParamsKey);
        if (!oauthParamsJson.IsNullOrEmpty)
        {
            try
            {
                var storedAuthState =
                    System.Text.Json.JsonSerializer.Deserialize<AuthState>(oauthParamsJson.ToString());
                if (storedAuthState != null)
                {
                    authState = storedAuthState;
                }

                // 删除已使用的参数
                await _redisDb.KeyDeleteAsync(oauthParamsKey);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to deserialize OAuth parameters from Redis");
            }
        }

        // 验证token是否有效
        var isValid = await ValidateToken(token, authState.ClientId);
        if (!isValid)
        {
            logger.LogWarning("Callback failed: invalid authentication token");
            return BadRequest("无效的认证令牌");
        }

        // 根据responseType决定返回方式
        if (authState.ResponseType == "code")
        {
            // 生成安全的授权码并存储在Redis中
            var authCode = GenerateSecureAuthCode();
            var codeKey = $"oauth:code:{authCode}";

            // 创建授权码信息对象（不包含实际的访问令牌）
            var authCodeInfo = new AuthCodeInfo
            {
                ClientId = authState.ClientId,
                RedirectUri = authState.RedirectUri,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                CodeChallenge = authState.CodeChallenge,
                CodeChallengeMethod = authState.CodeChallengeMethod,
                Scope = authState.Scope, // 添加scope信息
                Nonce = authState.Nonce // 添加nonce信息
            };

            // 存储到Redis，设置5分钟过期时间
            await _redisDb.StringSetAsync(
                codeKey,
                System.Text.Json.JsonSerializer.Serialize(authCodeInfo),
                TimeSpan.FromMinutes(5));

            logger.LogInformation("Authorization code {AuthCode} generated for user {UserId} and client {ClientId}",
                authCode, userId, authState.ClientId);

            // 重定向到第三方应用的回调地址
            var redirectUrl = $"{authState.RedirectUri}?code={authCode}&state={authState.State}";
            return Redirect(redirectUrl);
        }

        if (authState.ResponseType == "token")
        {
            logger.LogInformation("Direct token response for user {UserId} and client {ClientId}", userId,
                authState.ClientId);

            // 直接返回访问令牌
            var redirectUrl =
                $"{authState.RedirectUri}#access_token={token}&state={authState.State}&scope={Uri.EscapeDataString(authState.Scope)}";
            return Redirect(redirectUrl);
        }

        // 处理id_token响应类型 (Implicit Flow)
        if (authState.ResponseType is "id_token" or "id_token token")
        {
            // 检查scope是否包含openid
            if (!authState.Scope.Contains("openid"))
            {
                logger.LogWarning("Callback failed: openid scope is required for id_token response type");
                return BadRequest("需要openid scope才能返回id_token");
            }

            // 生成ID token
            var idToken = await GenerateIdToken(userId, authState.ClientId, authState.Nonce);

            if (string.IsNullOrEmpty(idToken))
            {
                logger.LogError("Failed to generate ID token for user {UserId} and client {ClientId}", userId,
                    authState.ClientId);
                return BadRequest("ID token生成失败");
            }

            logger.LogInformation("ID token generated for user {UserId} and client {ClientId}", userId,
                authState.ClientId);

            // 根据responseType决定返回方式
            if (authState.ResponseType == "id_token")
            {
                // 只返回ID token
                var redirectUrl = $"{authState.RedirectUri}#id_token={idToken}&state={authState.State}";
                return Redirect(redirectUrl);
            }
            else // id_token token
            {
                // 返回访问令牌和ID token
                var redirectUrl =
                    $"{authState.RedirectUri}#access_token={token}&id_token={idToken}&state={authState.State}&token_type=Bearer&expires_in=7200";
                return Redirect(redirectUrl);
            }
        }

        logger.LogWarning("Callback failed: unsupported response type {ResponseType}", authState.ResponseType);
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
    /// <param name="codeVerifier">PKCE代码验证器</param>
    /// <returns>访问令牌</returns>
    [HttpPost("token")]
    public async Task<IActionResult> Token(
        [FromForm(Name = "grant_type")] string grantType,
        [FromForm] string code,
        [FromForm(Name = "client_id")] string clientId,
        [FromForm(Name = "client_secret")] string clientSecret,
        [FromForm(Name = "redirect_uri")] string redirectUri,
        [FromForm(Name = "code_verifier")] string? codeVerifier = null)
    {
        logger.LogInformation("Token exchange request received for client {ClientId}", clientId);

        // 添加参数验证
        if (string.IsNullOrEmpty(grantType))
        {
            logger.LogWarning("Token exchange failed: missing grant_type parameter");
            return BadRequest(new { error = "invalid_request", error_description = "缺少必需参数: grant_type" });
        }

        if (grantType != "authorization_code")
        {
            logger.LogWarning("Token exchange failed: unsupported grant type {GrantType}", grantType);
            return BadRequest(new { error = "unsupported_grant_type", error_description = "不支持的授权类型: " + grantType });
        }

        if (string.IsNullOrEmpty(code))
        {
            logger.LogWarning("Token exchange failed: missing code parameter");
            return BadRequest(new { error = "invalid_request", error_description = "缺少必需参数: code" });
        }

        if (string.IsNullOrEmpty(clientId))
        {
            logger.LogWarning("Token exchange failed: missing client_id parameter");
            return BadRequest(new { error = "invalid_request", error_description = "缺少必需参数: client_id" });
        }

        if (string.IsNullOrEmpty(clientSecret))
        {
            logger.LogWarning("Token exchange failed: missing client_secret parameter");
            return BadRequest(new { error = "invalid_request", error_description = "缺少必需参数: client_secret" });
        }

        if (string.IsNullOrEmpty(redirectUri))
        {
            logger.LogWarning("Token exchange failed: missing redirect_uri parameter");
            return BadRequest(new { error = "invalid_request", error_description = "缺少必需参数: redirect_uri" });
        }

        // 验证客户端凭据
        var clientApp = await clientAppRepository.ValidateCredentialsAsync(clientId, clientSecret);
        if (clientApp == null)
        {
            logger.LogWarning("Token exchange failed: invalid client credentials for client {ClientId}", clientId);
            return Unauthorized(new { error = "invalid_client", error_description = "无效的客户端凭据" });
        }

        // 验证回调URL是否匹配
        if (!clientApp.IsRedirectUriValid(redirectUri))
        {
            logger.LogWarning("Token exchange failed: invalid redirect URI {RedirectUri} for client {ClientId}",
                redirectUri, clientId);
            return BadRequest(new { error = "invalid_request", error_description = "无效的回调地址" });
        }

        // 从Redis中获取授权码信息
        var codeKey = $"oauth:code:{code}";
        var authCodeInfoJson = await _redisDb.StringGetAsync(codeKey);

        if (authCodeInfoJson.IsNullOrEmpty)
        {
            logger.LogWarning("Token exchange failed: invalid authorization code {Code}", code);
            return BadRequest(new { error = "invalid_grant", error_description = "无效的授权码" });
        }

        try
        {
            var authCodeInfo = System.Text.Json.JsonSerializer.Deserialize<AuthCodeInfo>(authCodeInfoJson.ToString());

            if (authCodeInfo == null)
            {
                logger.LogWarning(
                    "Token exchange failed: unable to deserialize authorization code info for code {Code}", code);
                return BadRequest(new { error = "invalid_grant", error_description = "无效的授权码" });
            }

            // 验证授权码与请求参数是否匹配
            if (authCodeInfo.ClientId != clientId || authCodeInfo.RedirectUri != redirectUri)
            {
                logger.LogWarning("Token exchange failed: authorization code {Code} does not match request parameters",
                    code);
                return BadRequest(new { error = "invalid_grant", error_description = "授权码与请求参数不匹配" });
            }

            // 如果客户端支持PKCE，则要求提供code_verifier参数
            if (clientApp.SupportsPkce && string.IsNullOrEmpty(codeVerifier))
            {
                logger.LogWarning(
                    "Token exchange failed: PKCE is required for client {ClientId} but code_verifier is missing",
                    clientId);
                return BadRequest(new
                    { error = "invalid_request", error_description = "客户端要求使用PKCE，必须提供code_verifier参数" });
            }

            // 如果授权码有PKCE要求，验证code_verifier
            if (!string.IsNullOrEmpty(authCodeInfo.CodeChallenge))
            {
                if (string.IsNullOrEmpty(codeVerifier))
                {
                    logger.LogWarning(
                        "Token exchange failed: missing code_verifier for PKCE-enabled authorization code {Code}",
                        code);
                    return BadRequest(new { error = "invalid_request", error_description = "缺少必需参数: code_verifier" });
                }

                // 验证code_verifier长度
                if (codeVerifier.Length is < 43 or > 128)
                {
                    logger.LogWarning(
                        "Token exchange failed: invalid code_verifier length for authorization code {Code}", code);
                    return BadRequest(new { error = "invalid_request", error_description = "code_verifier长度无效" });
                }

                // 根据challenge method验证code_verifier
                if (authCodeInfo.CodeChallengeMethod == "S256")
                {
                    var challengeBytes = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));
                    var challenge = Convert.ToBase64String(challengeBytes)
                        .TrimEnd('=')
                        .Replace('+', '-')
                        .Replace('/', '_');

                    if (!string.Equals(challenge, authCodeInfo.CodeChallenge, StringComparison.Ordinal))
                    {
                        logger.LogWarning("Token exchange failed: invalid code_verifier for authorization code {Code}",
                            code);
                        return BadRequest(new { error = "invalid_grant", error_description = "无效的code_verifier" });
                    }
                }
                else if (authCodeInfo.CodeChallengeMethod == "plain")
                {
                    if (!string.Equals(codeVerifier, authCodeInfo.CodeChallenge, StringComparison.Ordinal))
                    {
                        logger.LogWarning("Token exchange failed: invalid code_verifier for authorization code {Code}",
                            code);
                        return BadRequest(new { error = "invalid_grant", error_description = "无效的code_verifier" });
                    }
                }
                else
                {
                    logger.LogWarning(
                        "Token exchange failed: unsupported code_challenge_method {Method} for authorization code {Code}",
                        authCodeInfo.CodeChallengeMethod, code);
                    return BadRequest(
                        new { error = "invalid_request", error_description = "不支持的code_challenge_method" });
                }
            }

            // 为用户生成访问令牌
            var member = await studentRepository.GetByIdAsync(authCodeInfo.UserId);
            if (member == null)
            {
                logger.LogWarning("Token exchange failed: user {UserId} not found", authCodeInfo.UserId);
                return BadRequest(new { error = "invalid_grant", error_description = "用户不存在" });
            }

            var token = await loginService.GetToken(member.UserId, clientId);
            if (string.IsNullOrEmpty(token))
            {
                logger.LogError("Token exchange failed: unable to generate token for user {UserId}",
                    authCodeInfo.UserId);
                return BadRequest(new { error = "server_error", error_description = "令牌生成失败" });
            }

            // 生成ID token（如果scope包含openid）
            string? idToken = null;
            if (authCodeInfo.Scope.Contains("openid"))
            {
                idToken = await GenerateIdToken(member.UserId, clientId, authCodeInfo.Nonce);
                if (string.IsNullOrEmpty(idToken))
                {
                    logger.LogError("Token exchange failed: unable to generate ID token for user {UserId}",
                        authCodeInfo.UserId);
                    return BadRequest(new { error = "server_error", error_description = "ID令牌生成失败" });
                }
            }

            // 删除已使用的授权码（一次性使用）
            await _redisDb.KeyDeleteAsync(codeKey);

            logger.LogInformation("Token exchange successful for user {UserId} with client {ClientId}",
                authCodeInfo.UserId, clientId);

            // 返回令牌信息，包括scope
            var response = new Dictionary<string, object>
            {
                ["access_token"] = token,
                ["token_type"] = "Bearer",
                ["expires_in"] = 7200, // 2小时
                ["scope"] = string.IsNullOrEmpty(authCodeInfo.Scope) ? DefaultScore : authCodeInfo.Scope
            };

            // 如果生成了ID token，则添加到响应中
            if (!string.IsNullOrEmpty(idToken))
            {
                response["id_token"] = idToken;
            }

            logger.LogInformation("Token exchange successful for user {UserId} with client {ClientId}",
                authCodeInfo.UserId, clientId);

            logger.LogInformation("access token is {token} , id token is {idToken}", token, idToken);

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Token exchange failed with exception for code {Code}", code);
            return BadRequest(new { error = "invalid_grant", error_description = "无效的授权码" });
        }
    }

    /// <summary>
    /// 验证访问令牌
    /// </summary>
    /// <param name="token">要验证的令牌</param>
    /// <param name="clientId">客户端 ID</param>
    /// <returns>令牌是否有效</returns>
    private async Task<bool> ValidateToken(string token, string clientId)
    {
        logger.LogDebug("Validating token");

        // 检查令牌是否为空
        if (string.IsNullOrEmpty(token))
        {
            logger.LogWarning("Token validation failed: token is null or empty");
            return false;
        }

        try
        {
            // 使用JwtHelper中的配置来验证令牌
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            // 检查令牌是否过期
            if (jsonToken.ValidTo < DateTime.UtcNow)
            {
                logger.LogWarning("Token validation failed: token expired at {ExpiryTime}", jsonToken.ValidTo);
                return false;
            }

            // 从令牌中提取用户ID
            var userId = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                logger.LogWarning("Token validation failed: missing user ID in token claims");
                return false;
            }

            // 使用loginService来验证令牌
            var isValid = await loginService.ValidateToken(userId, token, clientId);
            logger.LogInformation("Token validation result for user {UserId}: {IsValid}", userId, isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Token validation failed with exception");
            // 如果解析令牌时出现任何异常，认为令牌无效
            return false;
        }
    }

    /// <summary>
    /// 生成ID token
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="clientId">客户端ID</param>
    /// <param name="nonce">Nonce值</param>
    /// <returns>ID token</returns>
    private async Task<string?> GenerateIdToken(string userId, string clientId, string nonce)
    {
        try
        {
            var member = await studentRepository.GetByIdAsync(userId);
            if (member == null)
            {
                logger.LogWarning("Failed to generate ID token: user {UserId} not found", userId);
                return null;
            }

            var identity = "Member";

            var staff = await staffRepository.GetStaffByIdWithoutOtherData(userId);
            if (staff != null)
            {
                identity = staff.Identity;
            }

            var now = DateTime.UtcNow;
            var jwtId = Guid.NewGuid().ToString(); // 用于防止重放攻击

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new(JwtRegisteredClaimNames.UniqueName, member.UserName),
                new(ClaimTypes.Role, identity),
                new(ClaimTypes.NameIdentifier, userId),
                new(JwtRegisteredClaimNames.Jti, jwtId), // JWT ID 防止重放攻击
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
                new("at_hash", Guid.NewGuid().ToString()[..8]) // 访问令牌的哈希值（简化版）
            };

            // 如果提供了nonce，则添加到claims中
            if (!string.IsNullOrEmpty(nonce))
            {
                claims.Add(new Claim("nonce", nonce));
            }

            const string issuer = "https://api.xauat.site";
            var audience = clientId;

            var rsaKey = GetRsaKey();
            // Use RSA key for signing instead of HMAC
            var signingCredentials = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: now.AddHours(2), // ID token有效期2小时
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate ID token for user {UserId} and client {ClientId}", userId,
                clientId);
            return null;
        }
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <returns>用户信息</returns>
    [HttpGet("userinfo")]
    public async Task<IActionResult> UserInfo([FromQuery(Name = "access_token")] string? accessToken = "")
    {
        logger.LogInformation("User info request received");

        var token = HttpContext.GetJwt();

        if (!string.IsNullOrEmpty(accessToken))
        {
            token = accessToken;
        }

        if (string.IsNullOrEmpty(token))
        {
            logger.LogWarning("No access token provided");
            return Unauthorized();
        }

        // 验证token格式和签名
        var tokenHandler = new JwtSecurityTokenHandler();
        var secretKey = Environment.GetEnvironmentVariable("SECRETKEY", EnvironmentVariableTarget.Process) ??
                        config["Jwt:SecretKey"] ?? "";
        var key = Encoding.UTF8.GetBytes(secretKey);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = "iOS Club of XAUAT",
            ValidateAudience = true,
            ValidAudience = "iOS Club of XAUAT",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // 尝试验证token
        var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

        // 额外验证：确保token没有过期
        var jwtToken = (JwtSecurityToken)validatedToken;
        if (jwtToken.ValidTo < DateTime.UtcNow) return Unauthorized();

        // 验证token是否在Redis中存在（防止已注销的token继续使用）
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var clientId = claimsPrincipal.FindFirst("client_id")?.Value ?? "";
        if (!string.IsNullOrEmpty(userId))
        {
            var isValid = loginService.ValidateToken(userId, token, clientId).Result;
            if (!isValid) return Unauthorized();
        }
        else
        {
            return Unauthorized();
        }

        var member = await studentRepository.GetByIdAsync(userId);

        if (member == null)
        {
            logger.LogWarning("User info request failed: user {UserId} not found", userId);
            return Unauthorized();
        }

        logger.LogInformation("User info request successful for user {UserId}", userId);

        var identity = "Member";
        var staff = await staffRepository.GetStaffByIdWithoutOtherData(userId);
        if (staff != null)
        {
            identity = staff.Identity;
        }

        // 获取访问令牌中的scope信息
        var scopes = DefaultScore.Split(" ").ToList(); // 默认包含openid
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                // 获取scope信息
                var scopeClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "scope")?.Value;
                if (!string.IsNullOrEmpty(scopeClaim))
                {
                    scopes = scopeClaim
                        .Split(scopeClaim.Contains(',') ? ',' : ' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to parse token for scope information");
            }
        }
        else
        {
            logger.LogWarning("Failed to get scope information from token");
            return BadRequest("token is not found");
        }

        // 根据scope返回不同的用户信息
        var userInfo = new Dictionary<string, object>
        {
            ["sub"] = userId
        };

        // read profile email

        // profile scope - 基本信息
        if (scopes.Contains("profile") || scopes.Contains("full"))
        {
            userInfo.TryAdd("name", member.UserName);
            userInfo.TryAdd("nickname", member.UserName);
            userInfo.TryAdd("academy", member.Academy);
            userInfo.TryAdd("class", member.ClassName);
            userInfo.TryAdd("joinTime", member.JoinTime.ToString("yyyy-MM-dd"));
            userInfo.TryAdd("avatar", $"https://www.xauat.site/assets/Centre/{member.Gender}生.png");
        }

        // email scope - 邮箱信息
        if (scopes.Contains("email") || scopes.Contains("full"))
        {
            userInfo.TryAdd("email", member.EMail ?? "");
        }

        // read scope - 角色信息
        if (scopes.Contains("read") || scopes.Contains("full"))
        {
            userInfo.TryAdd("role", identity);
        }

        // phone scope - 手机信息
        if (scopes.Contains("phone") || scopes.Contains("full"))
        {
            userInfo.TryAdd("phone", member.PhoneNum);
        }

        return Ok(userInfo);
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
        logger.LogInformation("Store session request received");

        try
        {
            // 获取当前认证的用户信息
            var user = HttpContext.User.GetUser();
            if (user == null)
            {
                logger.LogWarning("Store session failed: user not authenticated");
                return Unauthorized("用户未认证");
            }

            // 从请求头获取token
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var token = authHeader.StartsWith("Bearer ") ? authHeader["Bearer ".Length..].Trim() : authHeader;

            if (string.IsNullOrEmpty(request.State))
            {
                logger.LogInformation("Session stored successfully for user {UserId} without Redis storage",
                    user.UserId);
                return Ok(new { success = true, message = "会话存储成功" });
            }

            try
            {
                // 解密state参数以获取原始state值作为Redis键
                var decryptedState = Encoding.UTF8.GetString(Convert.FromBase64String(request.State));
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

                logger.LogInformation("Session stored successfully for user {UserId} with Redis key {RedisKey}",
                    user.UserId, redisKey);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to store session in Redis for user {UserId}", user.UserId);
            }

            return Ok(new { success = true, message = "会话存储成功" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Store session failed with exception");
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
        logger.LogInformation("Client info request received for client {ClientId}", clientId);

        if (string.IsNullOrEmpty(clientId))
        {
            logger.LogWarning("Client info request failed: invalid client ID");
            return BadRequest("无效的客户端ID");
        }

        var clientApp = await clientAppRepository.GetByClientIdAsync(clientId);
        if (clientApp is not { IsActive: true })
        {
            logger.LogWarning("Client info request failed: client {ClientId} not found or inactive", clientId);
            return NotFound("客户端应用不存在或已禁用");
        }

        logger.LogInformation("Client info request successful for client {ClientId}", clientId);

        return Ok(new
        {
            name = clientApp.ApplicationName,
            description = clientApp.Description,
            client_id = clientApp.ClientId
        });
    }

    /// <summary>
    /// 从主站JWT获取会话
    /// </summary>
    /// <param name="request">会话需要的数据</param>
    /// <param name="scope">权限类别</param>
    /// <returns>重定向</returns>
    [HttpPost("from_main_jwt")]
    [Authorize]
    public async Task<IActionResult> FromMainJwt([FromBody] StoreSessionRequest request, [FromQuery] string scope)
    {
        var jwt = HttpContext.GetJwt();
        if (string.IsNullOrEmpty(jwt))
        {
            return Unauthorized("无效的JWT令牌");
        }

        var user = HttpContext.User.GetUser();
        if (user == null)
        {
            return Unauthorized("用户未认证");
        }

        var token = await loginService.LoginThirdPartyFromMainJwt(user.UserId, request.ClientId, jwt, scope);

        if (string.IsNullOrEmpty(token))
        {
            return BadRequest();
        }

        HttpContext.Request.Headers.Authorization = $"Bearer {token}";
        return Redirect($"/SSO/store-session?state={request.State}");
    }

    /// <summary>
    /// 生成安全的授权码
    /// </summary>
    /// <returns>安全的授权码</returns>
    private static string GenerateSecureAuthCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        // 使用URL安全的Base64编码
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// 授权状态信息
    /// </summary>
    [Serializable]
    public class AuthState
    {
        public string ClientId { get; set; } = "";
        public string RedirectUri { get; set; } = "";
        public string State { get; set; } = "";
        public string ResponseType { get; set; } = "";
        public string CodeChallenge { get; set; } = "";
        public string CodeChallengeMethod { get; set; } = "";
        public string Scope { get; set; } = ""; // 添加Scope支持
        public string Nonce { get; set; } = ""; // 添加Nonce支持
    }

    /// <summary>
    /// OAuth用户信息
    /// </summary>
    [Serializable]
    public class OAuthUserInfo
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Token { get; set; } = "";
    }

    /// <summary>
    /// 授权码信息
    /// </summary>
    [Serializable]
    public class AuthCodeInfo
    {
        public string ClientId { get; set; } = "";
        public string RedirectUri { get; set; } = "";
        public string UserId { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string CodeChallenge { get; set; } = "";
        public string CodeChallengeMethod { get; set; } = "";
        public string Scope { get; set; } = ""; // 添加Scope支持
        public string Nonce { get; set; } = ""; // 添加Nonce支持
    }
}