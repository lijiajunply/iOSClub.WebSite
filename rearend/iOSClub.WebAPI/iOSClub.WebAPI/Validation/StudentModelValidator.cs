using FluentValidation;
using iOSClub.Data.DataModels;

namespace iOSClub.WebAPI.Validation;

public class StudentModelValidator : AbstractValidator<StudentModel>
{
    public StudentModelValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("用户名不能为空")
            .MaximumLength(50).WithMessage("用户名长度不能超过50个字符")
            .Matches("^[a-zA-Z0-9\\u4e00-\\u9fa5]+$").WithMessage("用户名只能包含字母、数字和中文");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("学号不能为空")
            .Length(10).WithMessage("学号必须为10位")
            .Matches("^[0-9]+$").WithMessage("学号只能包含数字");

        RuleFor(x => x.Academy)
            .NotEmpty().WithMessage("学院不能为空")
            .MaximumLength(50).WithMessage("学院名称长度不能超过50个字符");

        RuleFor(x => x.PoliticalLandscape)
            .NotEmpty().WithMessage("政治面貌不能为空")
            .MaximumLength(10).WithMessage("政治面貌长度不能超过10个字符")
            .Must(x => new[] { "群众", "共青团员", "预备党员", "中共党员" }.Contains(x)).WithMessage("政治面貌必须是群众、共青团员、预备党员或中共党员");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("性别不能为空")
            .Length(1, 2).WithMessage("性别只能是1-2个字符")
            .Must(x => new[] { "男", "女" }.Contains(x)).WithMessage("性别必须是男或女");

        RuleFor(x => x.ClassName)
            .NotEmpty().WithMessage("班级不能为空")
            .MaximumLength(20).WithMessage("班级名称长度不能超过20个字符");

        RuleFor(x => x.PhoneNum)
            .NotEmpty().WithMessage("电话号码不能为空")
            .Length(11).WithMessage("电话号码必须为11位")
            .Matches("^1[3-9]\\d{9}$").WithMessage("电话号码格式不正确");

        RuleFor(x => x.EMail)
            .EmailAddress().WithMessage("邮箱格式不正确")
            .MaximumLength(256).WithMessage("邮箱长度不能超过256个字符")
            .When(x => !string.IsNullOrEmpty(x.EMail));

        RuleFor(x => x.PasswordHash)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(6).WithMessage("密码长度不能少于6个字符")
            .MaximumLength(256).WithMessage("密码长度不能超过256个字符");
    }
}