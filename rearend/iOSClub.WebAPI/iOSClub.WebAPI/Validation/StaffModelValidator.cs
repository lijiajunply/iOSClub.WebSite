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

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("员工姓名不能为空")
            .MaximumLength(50).WithMessage("员工姓名长度不能超过50个字符")
            .Matches("^[a-zA-Z0-9\\u4e00-\\u9fa5]+$").WithMessage("员工姓名只能包含字母、数字和中文");

        RuleFor(x => x.Identity)
            .NotEmpty().WithMessage("身份标识不能为空")
            .MaximumLength(20).WithMessage("身份标识长度不能超过20个字符")
            .Must(x => new[] { "Founder", "President", "Minister", "Department", "Member" }.Contains(x)).WithMessage("身份标识必须是Founder、President、Minister、Department或Member");
    }
}