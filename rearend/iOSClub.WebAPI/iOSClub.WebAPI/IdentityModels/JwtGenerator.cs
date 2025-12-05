using iOSClub.Data.ShowModels;
using iOSClub.DataApi.Services;
using iOSClub.WebAPI.Common.Config;
using iOSClub.WebAPI.Common.Security;

namespace iOSClub.WebAPI.IdentityModels;

public class JwtGenerator : ITokenGenerator
{
    private readonly JwtService _jwtService;
    
    public JwtGenerator(JwtConfig jwtConfig, RsaKeyManager rsaKeyManager, ILoggerFactory loggerFactory)
    {
        var jwtServiceLogger = loggerFactory.CreateLogger<JwtService>();
        _jwtService = new JwtService(jwtConfig, rsaKeyManager, jwtServiceLogger);
    }
    
    public (string AccessToken, string RefreshToken) GetMemberToken(MemberModel model, bool rememberMe = false, string scope = "", string clientId = "")
    {
        // 使用新的JwtService生成访问令牌和刷新令牌
        return _jwtService.GenerateTokens(model, scope, clientId);
    }
}