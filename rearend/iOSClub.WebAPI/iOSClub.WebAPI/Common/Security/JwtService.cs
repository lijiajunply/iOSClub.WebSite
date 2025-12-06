using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using iOSClub.Data.ShowModels;
using iOSClub.WebAPI.Common.Config;
using Microsoft.IdentityModel.Tokens;

namespace iOSClub.WebAPI.Common.Security;

/// <summary>
/// JWT服务类，用于生成和验证JWT令牌
/// </summary>
public class JwtService(
    JwtConfig jwtConfig,
    RsaKeyManager rsaKeyManager,
    ILogger<JwtService> logger)
{
    /// <summary>
    /// 生成访问令牌和刷新令牌
    /// </summary>
    /// <param name="memberModel">用户信息模型</param>
    /// <param name="scope">权限范围</param>
    /// <param name="clientId">客户端ID</param>
    /// <returns>访问令牌和刷新令牌</returns>
    public (string AccessToken, string RefreshToken) GenerateTokens(MemberModel memberModel, string scope = "",
        string clientId = "")
    {
        try
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("开始生成访问令牌和刷新令牌，用户ID：{UserId}", memberModel.UserId);
            }

            // 生成访问令牌
            var accessToken = GenerateAccessToken(memberModel, scope, clientId);

            // 生成刷新令牌
            var refreshToken = GenerateRefreshToken();

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("访问令牌和刷新令牌生成成功，用户ID：{UserId}", memberModel.UserId);
            }
            return (accessToken, refreshToken);
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "生成令牌失败，用户ID：{UserId}", memberModel.UserId);
            }
            throw;
        }
    }

    /// <summary>
    /// 生成访问令牌
    /// </summary>
    /// <param name="memberModel">用户信息模型</param>
    /// <param name="scope">权限范围</param>
    /// <param name="clientId">客户端ID</param>
    /// <returns>访问令牌</returns>
    private string GenerateAccessToken(MemberModel memberModel, string scope = "", string clientId = "")
    {
        var now = DateTime.UtcNow;
        var jwtId = Guid.NewGuid().ToString(); // 用于防止重放攻击

        scope = string.IsNullOrEmpty(scope) ? "full" : scope;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, memberModel.UserId),
            new Claim(JwtRegisteredClaimNames.UniqueName, memberModel.UserName),
            new Claim(ClaimTypes.Role, memberModel.Identity),
            new Claim(ClaimTypes.NameIdentifier, memberModel.UserId),
            new Claim(JwtRegisteredClaimNames.Jti, jwtId), // JWT ID 防止重放攻击
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new Claim("scope", scope),
            new Claim("client_id", clientId),
            new Claim("usage", "access") // 令牌使用场景：访问令牌
        };

        // 获取当前有效的RSA私钥
        var rsa = rsaKeyManager.GetCurrentPrivateKey();
        // 从RSA密钥中导出公钥的SHA256哈希值作为KeyId
        var publicKeyBytes = rsa.ExportRSAPublicKey();
        var keyId = Convert.ToBase64String(SHA256.HashData(publicKeyBytes)).Substring(0, 16);

        var rsaSecurityKey = new RsaSecurityKey(rsa) { KeyId = keyId };
        var signingCredentials = new SigningCredentials(
            rsaSecurityKey,
            SecurityAlgorithms.RsaSha256Signature);

        var securityToken = new JwtSecurityToken(
            issuer: jwtConfig.Issuer, // 签发者
            audience: jwtConfig.Audience, // 接收者
            claims: claims,
            notBefore: now, // 生效时间
            expires: now.AddMinutes(jwtConfig.AccessTokenExpiryMinutes), // 过期时间
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }

    /// <summary>
    /// 生成刷新令牌
    /// </summary>
    /// <returns>刷新令牌</returns>
    private string GenerateRefreshToken()
    {
        // 生成一个随机的GUID作为刷新令牌
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// 验证访问令牌
    /// </summary>
    /// <param name="token">访问令牌</param>
    /// <param name="validationParameters">验证参数</param>
    /// <returns>验证结果和声明列表</returns>
    public (bool IsValid, IEnumerable<Claim> Claims) ValidateAccessToken(string token,
        TokenValidationParameters? validationParameters = null)
    {
        try
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("开始验证访问令牌");
            }

            var tokenHandler = new JwtSecurityTokenHandler();

            // 如果没有提供验证参数，使用默认参数
            if (validationParameters == null)
            {
                validationParameters = GetDefaultValidationParameters();
            }

            // 验证令牌

            var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out _);

            // 检查令牌使用场景
            var usageClaim = claimsPrincipal.FindFirst("usage");
            if (usageClaim == null || usageClaim.Value != "access")
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("访问令牌使用场景验证失败");
                }
                return (false, Enumerable.Empty<Claim>());
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("访问令牌验证成功");
            }
            return (true, claimsPrincipal.Claims);
        }
        catch (SecurityTokenExpiredException)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError("访问令牌已过期");
            }
            // 重新抛出令牌过期异常，让中间件捕获处理
            throw;
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "访问令牌验证失败");
            }
            return (false, Enumerable.Empty<Claim>());
        }
    }

    /// <summary>
    /// 使用刷新令牌生成新的访问令牌
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    /// <param name="memberModel">用户信息模型</param>
    /// <param name="scope">权限范围</param>
    /// <param name="clientId">客户端ID</param>
    /// <returns>新的访问令牌</returns>
    public string RefreshAccessToken(string refreshToken, MemberModel memberModel, string scope = "",
        string clientId = "")
    {
        try
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("开始使用刷新令牌生成新的访问令牌，用户ID：{UserId}", memberModel.UserId);
            }

            // 这里可以添加刷新令牌的验证逻辑，例如检查黑名单
            // 实际实现中，应该从存储中获取刷新令牌并验证其有效性

            // 生成新的访问令牌
            var newAccessToken = GenerateAccessToken(memberModel, scope, clientId);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("刷新访问令牌成功，用户ID：{UserId}", memberModel.UserId);
            }
            return newAccessToken;
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "刷新访问令牌失败，用户ID：{UserId}", memberModel.UserId);
            }
            throw;
        }
    }

    /// <summary>
    /// 获取默认的令牌验证参数
    /// </summary>
    /// <returns>令牌验证参数</returns>
    public TokenValidationParameters GetDefaultValidationParameters()
    {
        // 获取当前有效的RSA公钥
        var rsa = rsaKeyManager.GetCurrentPublicKey();

        // 从RSA密钥中导出公钥的SHA256哈希值作为KeyId，与生成令牌时使用的KeyId保持一致
        var publicKeyBytes = rsa.ExportRSAPublicKey();
        var keyId = Convert.ToBase64String(SHA256.HashData(publicKeyBytes)).Substring(0, 16);
        var rsaSecurityKey = new RsaSecurityKey(rsa) { KeyId = keyId };

        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = rsaSecurityKey,
            ValidateIssuer = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtConfig.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1), // 允许1分钟的时钟偏差
            RequireExpirationTime = true,
            RequireSignedTokens = true
        };
    }

    /// <summary>
    /// 从访问令牌中提取声明
    /// </summary>
    /// <param name="token">访问令牌</param>
    /// <returns>声明列表</returns>
    public IEnumerable<Claim> ExtractClaimsFromToken(string token)
    {
        try
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("开始从访问令牌中提取声明");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("从访问令牌中提取声明成功");
            }
            return jwtToken.Claims;
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "从访问令牌中提取声明失败");
            }
            return [];
        }
    }
}