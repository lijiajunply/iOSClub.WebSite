using iOSClub.Data.ShowModels;

namespace iOSClub.DataApi.Services;

public interface IJwtHelper
{
    public string GetMemberToken(MemberModel model);
}

public interface ILoginService
{
    public Task<string> Login(LoginModel user);
    public Task<bool> Check(LoginModel user);
}

public class LoginService : ILoginService
{
    public async Task<string> Login(LoginModel user)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Check(LoginModel user)
    {
        throw new NotImplementedException();
    }
}