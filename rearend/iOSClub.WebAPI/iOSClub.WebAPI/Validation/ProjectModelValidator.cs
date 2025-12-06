using FluentValidation;
using iOSClub.Data.DataModels;

namespace iOSClub.WebAPI.Validation;

public class ProjectModelValidator : AbstractValidator<ProjectModel>
{
    public ProjectModelValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("项目ID不能为空")
            .MaximumLength(32).WithMessage("项目ID长度不能超过32个字符")
            .Matches("^[a-zA-Z0-9_\\-]+$").WithMessage("项目ID只能包含字母、数字、下划线和连字符");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("项目标题不能为空")
            .MaximumLength(20).WithMessage("项目标题长度不能超过20个字符");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("项目描述不能为空")
            .MaximumLength(512).WithMessage("项目描述长度不能超过512个字符");

        RuleFor(x => x.StartTime)
            .Matches("^\\d{4}-\\d{2}-\\d{2}$").WithMessage("开始时间格式必须为yyyy-MM-dd")
            .When(x => !string.IsNullOrEmpty(x.StartTime));

        RuleFor(x => x.EndTime)
            .Matches("^\\d{4}-\\d{2}-\\d{2}$").WithMessage("结束时间格式必须为yyyy-MM-dd")
            .GreaterThanOrEqualTo(x => x.StartTime).WithMessage("结束时间不能早于开始时间")
            .When(x => !string.IsNullOrEmpty(x.EndTime) && !string.IsNullOrEmpty(x.StartTime));
    }
}