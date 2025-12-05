using FluentValidation;
using iOSClub.Data.ShowModels;

namespace iOSClub.WebAPI.Validation;

public class LoginModelValidator : AbstractValidator<LoginModel>
{
    public LoginModelValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("用户ID不能为空")
            .MinimumLength(1).WithMessage("用户ID不能为空")
            .MaximumLength(20).WithMessage("用户ID长度不能超过20个字符");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(6).WithMessage("密码长度不能少于6个字符")
            .MaximumLength(256).WithMessage("密码长度不能超过256个字符")
            .Matches("^[a-zA-Z0-9!@#$%^&*()_+\\-=\\[\\]{};':\\\"\\|,.<>\\/?]+$").WithMessage("密码只能包含字母、数字和常见特殊字符");
    }
}