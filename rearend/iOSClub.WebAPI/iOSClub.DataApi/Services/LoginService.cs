using iOSClub.Data.ShowModels;
using iOSClub.DataApi.Repositories;
using StackExchange.Redis;
using System.Text.Json;
using iOSClub.Data;
using iOSClub.Data.DataModels;

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

    /// <summary>
    /// 修改用户密码
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="oldPassword">旧密码</param>
    /// <param name="newPassword">新密码</param>
    /// <returns>是否成功更改密码</returns>
    public Task<bool> ChangePassword(string userId, string oldPassword, string newPassword);

    /// <summary>
    /// 请求重置密码的验证码
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功发送验证码</returns>
    public Task<bool> RequestPasswordResetCode(string userId);

    /// <summary>
    /// 通过验证码重置密码
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="code">验证码</param>
    /// <param name="newPassword">新密码</param>
    /// <returns>是否成功重置密码</returns>
    public Task<bool> ResetPasswordWithCode(string userId, string code, string newPassword);
}

public class LoginService(
    IStudentRepository studentRepository,
    IStaffRepository staffRepository,
    IJwtHelper jwtHelper,
    IConnectionMultiplexer redis,
    IEmailService emailService)
    : ILoginService
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly IEmailService _emailService = emailService;

    private const string TokenPrefix = "token:";
    private const int TokenExpiryHours = 2; // 与JwtHelper中的过期时间保持一致

    public async Task<string> Login(string userId, string password)
    {
        if (!await studentRepository.Login(userId, password)) return "";

        var staff = await staffRepository.GetStaffByIdAsync(userId);
        var identity = "Member";
        if (staff != null)
        {
            identity = staff.Identity;
        }

        // 关于查询身份信息的，需要完成 StaffRepository 之后，在这里进行查询，我先随便给个值
        var memberModel = new MemberModel()
        {
            UserId = userId,
            Identity = identity,
            PasswordHash = password
        };

        var redisKey = $"{TokenPrefix}{userId}";
        var storedToken = await _db.StringGetAsync(redisKey);
        if (storedToken.HasValue && !string.IsNullOrEmpty(storedToken))
        {
            return storedToken.ToString();
        }

        var token = jwtHelper.GetMemberToken(memberModel);

        // 将token存储到Redis中，设置过期时间
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
            var userInfoKey = $"user:{userId}";

            // 同时删除用户信息
            await _db.KeyDeleteAsync([userInfoKey]);
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

        if (!storedToken.HasValue || string.IsNullOrEmpty(storedToken))
            return false;

        return storedToken == token;
    }

    public async Task<string> StaffLogin(string userId, string name)
    {
        var staff = await staffRepository.GetStaffByIdAsync(userId);

        if (staff == null || staff.Name != name)
            return "";

        var memberModel = new MemberModel()
        {
            UserName = staff.Name,
            UserId = staff.UserId,
            Identity = staff.Identity
        };

        var token = jwtHelper.GetMemberToken(memberModel);

        // 将token存储到Redis中，设置过期时间
        var redisKey = $"{TokenPrefix}{userId}";
        await _db.StringSetAsync(redisKey, token, TimeSpan.FromHours(TokenExpiryHours));

        // 存储用户信息，便于后续验证
        var userInfoKey = $"user:{userId}";
        await _db.StringSetAsync(userInfoKey, JsonSerializer.Serialize(memberModel),
            TimeSpan.FromHours(TokenExpiryHours));

        return token;
    }

    public async Task<bool> ChangePassword(string userId, string oldPassword, string newPassword)
    {
        // 验证旧密码是否正确
        var isLoginSuccess = await studentRepository.Login(userId, oldPassword);
        if (!isLoginSuccess)
            return false;

        // 获取用户信息
        var student = await studentRepository.GetByIdAsync(userId);
        if (student == null)
            return false;

        // 更新密码
        student.PasswordHash = DataTool.StringToHash(newPassword);
        var result = await studentRepository.UpdateAsync(MemberModel.AutoCopy<StudentModel, MemberModel>(student));

        return result;
    }

    public async Task<bool> RequestPasswordResetCode(string userId)
    {
        // 获取用户信息
        var student = await studentRepository.GetByIdAsync(userId);
        if (student == null)
            return false;

        // 检查是否有邮箱
        if (string.IsNullOrEmpty(student.EMail))
            return false;

        // 生成6位随机验证码
        var random = new Random();
        var code = random.Next(100000, 999999).ToString();

        // 将验证码存储到Redis，设置10分钟过期时间
        var redisKey = $"password_reset_code:{userId}";
        await _db.StringSetAsync(redisKey, code, TimeSpan.FromMinutes(10));

        // 发送邮件
        const string subject = "iOS Club 密码重置验证码";
        var body = $"""
                    您好 {student.UserName}，

                    您正在请求重置您的iOS Club账户密码。

                    请使用以下验证码完成密码重置操作：

                    {code}

                    此验证码将在10分钟后过期。

                    如果您没有请求密码重置，请忽略此邮件。

                    感谢您使用iOS Club！
                    """;

        var result = await _emailService.SendEmailAsync(student.EMail, subject, body);

        return result;
    }

    public async Task<bool> ResetPasswordWithCode(string userId, string code, string newPassword)
    {
        // 从Redis获取存储的验证码
        var redisKey = $"password_reset_code:{userId}";
        var storedCode = await _db.StringGetAsync(redisKey);

        // 检查验证码是否存在且匹配
        if (!storedCode.HasValue || storedCode != code)
            return false;

        // 获取用户信息
        var student = await studentRepository.GetByIdAsync(userId);
        if (student == null)
            return false;

        // 更新密码
        student.PasswordHash = DataTool.StringToHash(newPassword);
        var result = await studentRepository.UpdateAsync(MemberModel.AutoCopy<StudentModel, MemberModel>(student));

        // 删除已使用的验证码
        await _db.KeyDeleteAsync(redisKey);

        return result;
    }
}