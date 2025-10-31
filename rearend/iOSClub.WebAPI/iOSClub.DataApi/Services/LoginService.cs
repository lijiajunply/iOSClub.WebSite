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
    
    /// <summary>
    /// 员工登录
    /// </summary>
    /// <param name="userId">员工ID</param>
    /// <param name="name">员工姓名</param>
    /// <returns>凭证</returns>
    public Task<string> StaffLogin(string userId, string name);
}

public class LoginService : ILoginService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IJwtHelper _jwtHelper;
    private readonly IDatabase _db;
    
    private const string TokenPrefix = "token:";
    private const int TokenExpiryHours = 2; // 与JwtHelper中的过期时间保持一致

    public LoginService(IStudentRepository studentRepository, IStaffRepository staffRepository, IJwtHelper jwtHelper, IConnectionMultiplexer redis)
    {
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _jwtHelper = jwtHelper;
        _db = redis.GetDatabase();
    }

    public async Task<string> Login(string userId, string password)
    {
        if (!await _studentRepository.Login(userId, password)) return "";
        // 关于查询身份信息的，需要完成 StaffRepository 之后，在这里进行查询，我先随便给个值
        var memberModel = new MemberModel()
        {
            UserId = userId,
            Identity = "Member",
            PasswordHash = password
        };

        var token = _jwtHelper.GetMemberToken(memberModel);

        // 将token存储到Redis中，设置过期时间
        var redisKey = $"{TokenPrefix}{userId}";
        await _db.StringSetAsync(redisKey, token, TimeSpan.FromHours(TokenExpiryHours));

        // 存储用户信息，便于后续验证
        var userInfoKey = $"user:{userId}";
        await _db.StringSetAsync(userInfoKey, JsonSerializer.Serialize(memberModel),
            TimeSpan.FromHours(TokenExpiryHours));

        return token;
    }

    public async Task<bool> Logout(string userId)
    {
        try
        {
            var redisKey = $"{TokenPrefix}{userId}";
            var userInfoKey = $"user:{userId}";

            // 同时删除token和用户信息
            await _db.KeyDeleteAsync([redisKey, userInfoKey]);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ValidateToken(string userId, string token)
    {
        var redisKey = $"{TokenPrefix}{userId}";
        var storedToken = await _db.StringGetAsync(redisKey);

        // 验证token是否存在且匹配
        return !string.IsNullOrEmpty(storedToken) && storedToken == token;
    }
    
    public async Task<string> StaffLogin(string userId, string name)
    {
        var staff = await _staffRepository.GetStaffByIdAsync(userId);
        
        if (staff == null || staff.Name != name)
            return "";
        
        var memberModel = new MemberModel()
        {
            UserName = staff.Name,
            UserId = staff.UserId,
            Identity = staff.Identity
        };
        
        var token = _jwtHelper.GetMemberToken(memberModel);
        
        // 将token存储到Redis中，设置过期时间
        var redisKey = $"{TokenPrefix}{userId}";
        await _db.StringSetAsync(redisKey, token, TimeSpan.FromHours(TokenExpiryHours));
        
        // 存储用户信息，便于后续验证
        var userInfoKey = $"user:{userId}";
        await _db.StringSetAsync(userInfoKey, JsonSerializer.Serialize(memberModel),
            TimeSpan.FromHours(TokenExpiryHours));
        
        return token;
    }
}