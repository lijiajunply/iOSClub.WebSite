using iOSClub.Data.ShowModels;
using iOSClub.DataApi.Repositories;
using StackExchange.Redis;
using System.Text.Json;

namespace iOSClub.DataApi.Services;

public interface IJwtHelper
{
    public string GetMemberToken(MemberModel model);
}
public interface ILoginService
{
    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="userId">学号</param>
    /// <param name="password">密码（初始密码为手机号）</param>
    /// <returns>凭证</returns>
    public Task<string> Login(string userId, string password);
    
    /// <summary>
    /// 登出
    /// </summary>
    /// <param name="userId">学号</param>
    /// <returns>是否成功登出</returns>
    public Task<bool> Logout(string userId);
    
    /// <summary>
    /// 验证token是否有效
    /// </summary>
    /// <param name="userId">学号</param>
    /// <param name="token">token值</param>
    /// <returns>是否有效</returns>
    public Task<bool> ValidateToken(string userId, string token);
}

public class LoginService(IStudentRepository repository, IJwtHelper jwtHelper, IConnectionMultiplexer redis) : ILoginService
{
    private readonly IDatabase _db = redis.GetDatabase();
    private const string TokenPrefix = "token:";
    private const int TokenExpiryHours = 2; // 与JwtHelper中的过期时间保持一致
    
    public async Task<string> Login(string userId, string password)
    {
        if (await repository.Login(userId, password))
        {
            // 关于查询身份信息的，需要完成 StaffRepository 之后，在这里进行查询，我先随便给个值
            var memberModel = new MemberModel()
            {
                UserId = userId,
                Identity = "Member",
                PasswordHash = password
            };
            
            string token = jwtHelper.GetMemberToken(memberModel);
            
            // 将token存储到Redis中，设置过期时间
            string redisKey = $"{TokenPrefix}{userId}";
            await _db.StringSetAsync(redisKey, token, TimeSpan.FromHours(TokenExpiryHours));
            
            // 存储用户信息，便于后续验证
            string userInfoKey = $"user:{userId}";
            await _db.StringSetAsync(userInfoKey, JsonSerializer.Serialize(memberModel), TimeSpan.FromHours(TokenExpiryHours));
            
            return token;
        }

        return "";
    }

    public async Task<bool> Logout(string userId)
    {
        try
        {
            string redisKey = $"{TokenPrefix}{userId}";
            string userInfoKey = $"user:{userId}";
            
            // 同时删除token和用户信息
            await _db.KeyDeleteAsync(new RedisKey[] { redisKey, userInfoKey });
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public async Task<bool> ValidateToken(string userId, string token)
    {
        string redisKey = $"{TokenPrefix}{userId}";
        var storedToken = await _db.StringGetAsync(redisKey);
        
        // 验证token是否存在且匹配
        return !string.IsNullOrEmpty(storedToken) && storedToken == token;
    }
}