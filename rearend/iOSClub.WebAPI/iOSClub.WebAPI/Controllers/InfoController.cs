using iOSClub.Data;
using iOSClub.DataApi.ShowModels;
using iOSClub.WebAPI.IdentityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class InfoController(
    IDbContextFactory<iOSContext> factory,
    IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    [HttpGet("academies")]
    public ActionResult<string[]> GetAcademies() => SignRecord.Academies;

    [TokenActionFilter]
    [Authorize]
    [HttpGet("user-info")]
    public async Task<IActionResult> GetUserInfo()
    {
        await using var context = await factory.CreateDbContextAsync();

        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();
        var student = await context.Students.FirstOrDefaultAsync(x => x.UserId == member.UserId);
        if (student == null) return NotFound();

        if (member.Identity == "Member")
        {
            return Ok();
        }

        if (member.Identity == "Department")
        {
            return Ok(new
            {
                Tasks = await context.Tasks.Where(x => x.Users.Any(y => y.UserId == member.UserId))
                    .ToArrayAsync(),
                Projects = await context.Projects.Where(x => x.Staffs.Any(y => y.UserId == member.UserId))
                    .ToArrayAsync(),
            });
        }

        if (member.Identity == "Minister")
        {
            var staff = await context.Staffs.FirstOrDefaultAsync(x => x.UserId == member.UserId);

            if (staff == null) return NotFound();

            return Ok(new
            {
                Tasks = await context.Tasks.Where(x => x.Users.Any(y => y.UserId == member.UserId))
                    .ToArrayAsync(),
                Projects = await context.Projects
                    .Where(x => x.Staffs.Any(y => y.UserId == member.UserId) || x.Department == staff.Department)
                    .ToArrayAsync(),
                Resources = await context.Resources.ToArrayAsync(),
            });
        }

        return Ok(new
        {
            Total = await context.Students.CountAsync(),
            StaffsCount = await context.Staffs.CountAsync(),
            Projects = await context.Projects.ToArrayAsync(),
            Tasks = await context.Tasks.ToArrayAsync(),
            Resources = await context.Resources.ToArrayAsync(),
            Departments = await context.Departments.ToArrayAsync()
        });
    }
}