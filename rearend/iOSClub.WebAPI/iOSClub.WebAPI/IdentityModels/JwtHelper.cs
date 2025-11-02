using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using iOSClub.Data.ShowModels;
using iOSClub.DataApi.Services;
using Microsoft.IdentityModel.Tokens;

namespace iOSClub.WebAPI.IdentityModels;

public class JwtHelper(IConfiguration configuration) : IJwtHelper
{
    public string GetMemberToken(MemberModel model)
    {
        var now = DateTime.UtcNow;
        var jwtId = Guid.NewGuid().ToString(); // 用于防止重放攻击

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, model.UserId),
            new Claim(JwtRegisteredClaimNames.UniqueName, model.UserName),
            new Claim(ClaimTypes.Role, model.Identity),
            new Claim(ClaimTypes.NameIdentifier, model.UserId),
            new Claim(JwtRegisteredClaimNames.Jti, jwtId), // JWT ID 防止重放攻击
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));
        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var securityToken = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "iOSClub", // 签发者
            audience: configuration["Jwt:Audience"] ?? "iOSClubApp", // 接收者
            claims: claims,
            notBefore: now, // 生效时间
            expires: now.AddMinutes(int.Parse(configuration["Jwt:AccessTokenExpirationInMinutes"] ??
                                              "30")), // 过期时间（默认30分钟）
            signingCredentials: signingCredentials
        );
        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
}