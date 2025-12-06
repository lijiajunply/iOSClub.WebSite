using FluentValidation;
using iOSClub.Data.DataModels;

namespace iOSClub.WebAPI.Validation;

public class StaffModelValidator : AbstractValidator<StaffModel>
{
    public StaffModelValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("员工ID不能为空")
            .Length(10).WithMessage("员工ID必须为10位")
            .Matches("^[0-9]+$").WithMessage("员工ID只能包含数字");

        RuleFor(x => x.Identity)
            .NotEmpty().WithMessage("身份标识不能为空")
            .MaximumLength(20).WithMessage("身份标识长度不能超过20个字符")
            .Must(x => new[] { "Founder", "President", "Minister", "Department" }.Contains(x)).WithMessage("身份标识必须是Founder、President、Minister、Department");
    }
}