using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using iOSClub.Data.ShowModels;
using iOSClub.DataApi.Services;
using Microsoft.IdentityModel.Tokens;

namespace iOSClub.WebAPI.IdentityModels;

public class JwtHelper(IConfiguration configuration) : IJwtHelper
{
    public string GetMemberToken(MemberModel model, bool rememberMe = false)
    {
        var now = DateTime.UtcNow;
        var jwtId = Guid.NewGuid().ToString(); // 用于防止重放攻击

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, model.UserId),
            new Claim(JwtRegisteredClaimNames.UniqueName, model.UserName),
            new Claim(ClaimTypes.Role, model.Identity),
            new Claim(ClaimTypes.NameIdentifier, model.UserId),
            new Claim(ClaimTypes.Email, model.EMail ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, jwtId), // JWT ID 防止重放攻击
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));
        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var securityToken = new JwtSecurityToken(
            issuer: "iOS Club of XAUAT", // 签发者
            audience: "iOS Club of XAUAT", // 接收者
            claims: claims,
            notBefore: now, // 生效时间
            expires: now.AddHours(rememberMe ? 24 : 2),
            signingCredentials: signingCredentials
        );
        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
}