using FluentValidation;
using iOSClub.Data.DataModels;

namespace iOSClub.WebAPI.Validation;

public class ArticleModelValidator : AbstractValidator<ArticleModel>
{
    public ArticleModelValidator()
    {
        RuleFor(x => x.Path)
            .NotEmpty().WithMessage("文章路径不能为空")
            .MaximumLength(128).WithMessage("文章路径长度不能超过128个字符")
            .Matches("^[a-zA-Z0-9_\\-\\/]+$").WithMessage("文章路径只能包含字母、数字、下划线、连字符和斜杠");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("文章标题不能为空")
            .MaximumLength(32).WithMessage("文章标题长度不能超过32个字符");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("文章内容不能为空")
            .MinimumLength(10).WithMessage("文章内容不能少于10个字符");

        RuleFor(x => x.Identity)
            .MaximumLength(20).WithMessage("身份标识长度不能超过20个字符")
            .Must(x => string.IsNullOrEmpty(x) || new[] { "Founder", "President", "Minister", "Department" }.Contains(x)).WithMessage("身份标识必须是Founder、President、Minister或Department")
            .When(x => x.Identity != null);

        RuleFor(x => x.CategoryId)
            .MaximumLength(128).WithMessage("分类ID长度不能超过128个字符")
            .When(x => x.CategoryId != null);

        RuleFor(x => x.ArticleOrder)
            .GreaterThanOrEqualTo(0).WithMessage("文章排序必须大于或等于0")
            .LessThanOrEqualTo(9999).WithMessage("文章排序不能超过9999");
    }
}