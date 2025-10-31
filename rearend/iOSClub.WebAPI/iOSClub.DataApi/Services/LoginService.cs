using iOSClub.Data.ShowModels;
using iOSClub.DataApi.Repositories;

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
}

public class LoginService(IStudentRepository repository, IJwtHelper jwtHelper) : ILoginService
{
    public async Task<string> Login(string userId, string password)
    {
        if (await repository.Login(userId, password))
        {
            // 关于查询身份信息的，需要完成 StaffRepository 之后，在这里进行查询，我先随便给个值

            return jwtHelper.GetMemberToken(new MemberModel()
            {
                UserId = userId,
                Identity = "Member",
                PasswordHash = password
            });
        }

        return "";
    }

    public Task<bool> Logout(string userId)
    {
        throw new NotImplementedException();
    }
}