using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using iOSClub.Data;
using iOSClub.Data.ShowModels;
using iOSClub.DataApi.Services;
using Microsoft.IdentityModel.Tokens;

namespace iOSClub.WebAPI.IdentityModels;

public class JwtHelper(IConfiguration configuration) : IJwtHelper
{
    public string GetMemberToken(MemberModel model)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, model.UserName),
            new Claim(ClaimTypes.Role, model.Identity),
            new Claim(ClaimTypes.NameIdentifier,model.UserId)
        };

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));
        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var securityToken = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.Now, //notBefore
            expires: DateTime.Now.AddHours(2), //expires
            signingCredentials: signingCredentials
        );
        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }

    /// <summary>
    /// 将数据进行哈希(MD5)加密
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    private string GetStaffToken(MemberModel model)
    {
        return DataTool.StringToHash($"{model.UserName}/{model.UserId}");
    }

    /// <summary>
    /// 验证Model是否符合此哈希值
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public bool IsStaffContrastOK(string hash, MemberModel model)
    {
        return GetStaffToken(model) == hash;
    }
}