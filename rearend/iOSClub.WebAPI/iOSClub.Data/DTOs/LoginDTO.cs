namespace iOSClub.Data.DTOs;

public class LoginDTO
{
    public string UserId { get; set; } = "";
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}
