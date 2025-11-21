using iOSClub.Data.ShowModels;
using iOSClub.DataApi.Repositories;
using StackExchange.Redis;
using System.Text.Json;
using iOSClub.Data;
using Microsoft.Extensions.Logging;

namespace iOSClub.DataApi.Services;

public interface IJwtHelper
{
    public string GetMemberToken(MemberModel model, bool rememberMe = false, string scope = "", string clientId = "");
}

public interface ILoginService
{
    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="model">数据</param>
    /// <param name="clientId">客户端 ID</param>
    /// <param name="scope">权限</param>
    /// <returns>凭证</returns>
    public Task<string> Login(LoginModel model, string clientId = "", string scope = "");

    public Task<string> LoginThirdPartyFromMainJwt(string userId, string clientId, string jwt, string scope = "");

    /// <summary>
    /// 登出
    /// </summary>
    /// <param name="userId">学号</param>
    /// <param name="clientId">客户端 ID</param>
    /// <returns>是否成功登出</returns>
    public Task<bool> Logout(string userId, string clientId = "");

    /// <summary>
    /// 验证token是否有效
    /// </summary>
    /// <param name="userId">学号</param>
    /// <param name="token">token值</param>
    /// <param name="clientId">客户端 ID</param>
    /// <returns>是否有效</returns>
    public Task<bool> ValidateToken(string userId, string token, string clientId);

    /// <summary>
    /// 员工登录
    /// </summary>
    /// <param name="model">数据</param>
    /// <param name="clientId">客户端 ID</param>
    /// <param name="scope">权限</param>
    /// <returns>凭证</returns>
    public Task<string> StaffLogin(LoginModel model, string clientId = "", string scope = "");

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

    public Task<string> GetToken(string userId, string clientId = "");

    /// <summary>
    /// 验证token是否有效（简化版）
    /// </summary>
    /// <param name="userId">学号</param>
    /// <param name="token">token值</param>
    /// <returns>是否有效</returns>
    public Task<bool> ValidateToken(string userId, string token);

    // public Task<bool> ExtendTime(string userId, string token, string? clientId);
}

public class LoginService(
    IStudentRepository studentRepository,
    IStaffRepository staffRepository,
    IJwtHelper jwtHelper,
    IConnectionMultiplexer redis,
    IEmailService emailService,
    IClientApplicationRepository clientApplicationRepository,
    ILogger<LoginService> logger)
    : ILoginService
{
    private readonly IDatabase _db = redis.GetDatabase();

    private const string TokenPrefix = "token:";
    private const int TokenExpiryHours = 1; // 与JwtHelper中的过期时间保持一致

    public async Task<string> Login(LoginModel model, string clientId = "", string scope = "")
    {
        if (!await studentRepository.Login(model.UserId, model.Password)) return "";

        var staff = await staffRepository.GetStaffByIdWithoutOtherData(model.UserId);
        var identity = "Member";
        string name;
        var isHasEMail = false;
        if (staff != null)
        {
            identity = staff.Identity;
            name = staff.Name;
        }
        else
        {
            var stu = await studentRepository.GetByIdAsync(model.UserId);
            if (stu == null) return "";
            name = stu.UserName;
            isHasEMail = string.IsNullOrEmpty(stu.EMail);
        }

        // 关于查询身份信息的，需要完成 StaffRepository 之后，在这里进行查询，我先随便给个值
        var memberModel = new MemberModel()
        {
            UserName = name,
            UserId = model.UserId,
            Identity = identity,
            PasswordHash = model.Password
        };

        var s = "";

        if (!string.IsNullOrEmpty(clientId))
        {
            var app = await clientApplicationRepository.GetByClientIdAsync(clientId);
            if (app != null)
            {
                if (app.IsNeedEMail && !isHasEMail)
                {
                    throw new Exception("该应用需要你的邮箱信息");
                }

                s = $"client_id:{app.ClientSecret}";
            }
        }

        var redisKey = $"{TokenPrefix}{model.UserId}{s}";
        var storedToken = await _db.StringGetAsync(redisKey);
        if (storedToken.HasValue && !string.IsNullOrEmpty(storedToken))
        {
            return storedToken.ToString();
        }

        var token = jwtHelper.GetMemberToken(memberModel, model.RememberMe, scope, clientId);

        // 将token存储到Redis中，设置过期时间
        await _db.StringSetAsync(redisKey, token, TimeSpan.FromHours(TokenExpiryHours * (model.RememberMe ? 24 : 2)));

        // 存储用户信息，便于后续验证
        var userInfoKey = $"user:{model.UserId}{s}";
        await _db.StringSetAsync(userInfoKey, JsonSerializer.Serialize(memberModel),
            TimeSpan.FromHours(TokenExpiryHours * (model.RememberMe ? 24 : 2)));

        return token;
    }

    public async Task<string> LoginThirdPartyFromMainJwt(string userId, string clientId, string jwt, string scope = "")
    {
        if (!await ValidateToken(userId, jwt))
        {
            logger.LogError("Invalid JWT token");
            return "";
        }

        var app = await clientApplicationRepository.GetByClientIdAsync(clientId);
        if (app == null)
        {
            logger.LogError("Invalid client ID");
            return "";
        }

        var s = $"client_id:{app.ClientSecret}";

        var redisKey = $"{TokenPrefix}{userId}{s}";
        var storedToken = await _db.StringGetAsync(redisKey);
        if (storedToken.HasValue && !string.IsNullOrEmpty(storedToken))
        {
            logger.LogInformation("Token already exists, get the token");
            return storedToken.ToString();
        }

        var member = await studentRepository.GetByIdAsync(userId);
        var memberModel = new MemberModel()
        {
            UserId = userId
        };

        if (member != null)
        {
            memberModel.UserName = member.UserName;
            memberModel.PasswordHash = member.PasswordHash;
            memberModel.Identity = "Member";
        }

        var staff = await staffRepository.GetStaffByIdWithoutOtherData(userId);
        if (staff != null)
        {
            memberModel = new MemberModel()
            {
                UserName = staff.Name,
                Identity = staff.Identity
            };
        }

        var token = jwtHelper.GetMemberToken(memberModel, scope: scope, clientId: clientId);

        // 将token存储到Redis中，设置过期时间
        await _db.StringSetAsync(redisKey, token, TimeSpan.FromHours(TokenExpiryHours * 2));

        // 存储用户信息，便于后续验证
        var userInfoKey = $"user:{userId}{s}";
        await _db.StringSetAsync(userInfoKey, JsonSerializer.Serialize(memberModel),
            TimeSpan.FromHours(TokenExpiryHours * 2));

        return token;
    }

    public async Task<bool> Logout(string userId, string clientId = "")
    {
        try
        {
            var s = "";

            if (!string.IsNullOrEmpty(clientId))
            {
                var app = await clientApplicationRepository.GetByClientIdAsync(clientId);
                if (app != null)
                {
                    s = $"client_id:{app.ClientSecret}";
                }
            }

            // 同时删除用户信息
            await _db.KeyDeleteAsync([$"user:{userId}{s}", $"{TokenPrefix}{userId}{s}"]);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<string> GetToken(string userId, string clientId = "")
    {
        if (string.IsNullOrEmpty(userId)) return "";
        var s = "";

        if (!string.IsNullOrEmpty(clientId))
        {
            var app = await clientApplicationRepository.GetByClientIdAsync(clientId);
            if (app != null)
            {
                s = $"client_id:{app.ClientSecret}";
            }
        }

        var storedToken = await _db.StringGetAsync($"{TokenPrefix}{userId}{s}");
        return storedToken.HasValue && !string.IsNullOrEmpty(storedToken) ? storedToken.ToString() : "";
    }

    public Task<bool> ValidateToken(string userId, string token)
    {
        return ValidateToken(userId, token, "");
    }

    public async Task<bool> ValidateToken(string userId, string token, string clientId)
    {
        if (string.IsNullOrEmpty(userId)) return false;

        var s = "";

        if (!string.IsNullOrEmpty(clientId))
        {
            var app = await clientApplicationRepository.GetByClientIdAsync(clientId);
            if (app != null)
            {
                s = $"client_id:{app.ClientSecret}";
            }
        }

        var redisKey = $"{TokenPrefix}{userId}{s}";
        var storedToken = await _db.StringGetAsync(redisKey);

        if (!storedToken.HasValue || string.IsNullOrEmpty(storedToken))
            return false;

        return storedToken == token;
    }

    public async Task<string> StaffLogin(LoginModel model, string clientId = "", string scope = "")
    {
        var staff = await staffRepository.GetStaffByIdWithoutOtherData(model.Password);

        if (staff == null || staff.Name != model.UserId)
            return "";

        var memberModel = new MemberModel()
        {
            UserName = staff.Name,
            UserId = staff.UserId,
            Identity = staff.Identity
        };

        var token = jwtHelper.GetMemberToken(memberModel, model.RememberMe, scope, clientId: clientId);

        var s = "";

        if (!string.IsNullOrEmpty(clientId))
        {
            var app = await clientApplicationRepository.GetByClientIdAsync(clientId);
            if (app != null)
            {
                s = $"client_id:{app.ClientSecret}";
            }
        }

        var redisKey = $"{TokenPrefix}{model.Password}{s}";
        var storedToken = await _db.StringGetAsync(redisKey);
        if (storedToken.HasValue && !string.IsNullOrEmpty(storedToken))
        {
            return storedToken.ToString();
        }

        // 将token存储到Redis中，设置过期时间
        await _db.StringSetAsync(redisKey, token, TimeSpan.FromHours(TokenExpiryHours * (model.RememberMe ? 12 : 2)));

        // 存储用户信息，便于后续验证
        var userInfoKey = $"user:{model.Password}{s}";
        await _db.StringSetAsync(userInfoKey, JsonSerializer.Serialize(memberModel),
            TimeSpan.FromHours(TokenExpiryHours * (model.RememberMe ? 12 : 2)));

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
        var result = await studentRepository.UpdateAsync(student);

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

        var redisKey = $"password_reset_code:{userId}";
        string code;
        var storedCode = await _db.StringGetAsync(redisKey);
        if (!storedCode.HasValue || string.IsNullOrEmpty(storedCode))
        {
            // 生成6位随机验证码
            var random = new Random();
            code = random.Next(100000, 999999).ToString();
            // 将验证码存储到Redis，设置10分钟过期时间

            await _db.StringSetAsync(redisKey, code, TimeSpan.FromMinutes(10));
        }
        else
        {
            code = storedCode.ToString();
        }

        // 发送邮件
        const string subject = "iOS Club 密码重置验证码";
        var body = GeneratePasswordResetEmailBody(student.UserName, code);

        var result = await emailService.SendEmailAsync(student.EMail, subject, body, true);

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
        var result = await studentRepository.UpdateAsync(student);

        // 删除已使用的验证码
        if (result) await _db.KeyDeleteAsync(redisKey);

        return result;
    }

    /// <summary>
    /// 生成密码重置邮件内容
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="code">验证码</param>
    /// <returns>格式化后的邮件内容</returns>
    private static string GeneratePasswordResetEmailBody(string userName, string code)
    {
        return $@"<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f8f8f8;
        }}
        .container {{
            background-color: white;
            padding: 40px;
            border-radius: 12px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.05);
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .code {{
            font-size: 32px;
            font-weight: 600;
            text-align: center;
            letter-spacing: 8px;
            padding: 20px;
            background-color: #f1f1f1;
            border-radius: 8px;
            margin: 30px 0;
            color: #111;
        }}
        .footer {{
            margin-top: 30px;
            font-size: 14px;
            color: #888;
            text-align: center;
        }}
        hr {{
            border: none;
            height: 1px;
            background-color: #eee;
            margin: 30px 0;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>iOS Club</h1>
            <p>密码重置请求</p>
        </div>
        
        <p>尊敬的 {userName}，</p>
        
        <p>您正在请求重置您的 iOS Club 账户密码。</p>
        
        <p>请在密码重置页面输入以下验证码：</p>
        
        <div class=""code"">{code}</div>
        
        <p>此验证码将在 10 分钟后过期。</p>
        
        <hr>
        
        <p style=""font-size: 14px; color: #666;"">如果您没有请求密码重置，请忽略此邮件。您的账户安全不会受到影响。</p>
        
        <div class=""footer"">
            <p style=""margin-top: 20px; font-size: 12px;"">© 2025 iOS Club of XAUAT. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
}