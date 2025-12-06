using iOSClub.Data.DataModels;

namespace iOSClub.DataApi.CQRS.Commands;

// 命令：创建文章
public record CreateArticleCommand(ArticleModel Article) : ICommand;

// 命令：更新文章
public record UpdateArticleCommand(ArticleModel Article) : ICommand;

// 命令：删除文章
public record DeleteArticleCommand(string Id) : ICommand;

// 命令：批量更新文章
public record UpdateManyArticlesCommand(List<ArticleModel> Articles) : ICommand;