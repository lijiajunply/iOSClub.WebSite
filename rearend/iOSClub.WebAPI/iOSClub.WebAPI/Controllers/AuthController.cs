using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoginModel = iOSClub.Data.ShowModels.LoginModel;
using MemberModel = iOSClub.Data.ShowModels.MemberModel;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    IDbContextFactory<iOSContext> factory,
    IJwtHelper jwtHelper)
    : ControllerBase
{
    [HttpPost("signup")]
    public async Task<ActionResult<string>> SignUp(StudentModel model)
    {
        await using var context = await factory.CreateDbContextAsync();
        if (context.Students == null!)
        {
            return Problem("Entity set 'MemberContext.Students'  is null.");
        }

        context.Students.Add(model);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (await context.Students.AnyAsync(e => e.UserId == model.UserId))
                return Conflict();

            throw;
        }

        return jwtHelper.GetMemberToken(MemberModel.AutoCopy<StudentModel, MemberModel>(model));
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginModel loginModel)
    {
        await using var context = await factory.CreateDbContextAsync();
        if (context.Students == null!)
            return NotFound();

        var peo = await context.Staffs.FirstOrDefaultAsync(x =>
            x.UserId == loginModel.Id && x.Name == loginModel.Name);

        var id = peo?.Identity ?? "Member";

        var model =
            await context.Students.FirstOrDefaultAsync(x =>
                x.UserId == loginModel.Id && x.UserName == loginModel.Name);

        if (model == null)
        {
            if (peo != null)
            {
                return jwtHelper.GetMemberToken(new MemberModel()
                    { UserName = peo.Name, UserId = peo.UserId, Identity = peo.Identity });
            }

            return NotFound();
        }

        var member = MemberModel.AutoCopy<StudentModel, MemberModel>(model);
        member.Identity = id;

        return jwtHelper.GetMemberToken(member);
    }
}