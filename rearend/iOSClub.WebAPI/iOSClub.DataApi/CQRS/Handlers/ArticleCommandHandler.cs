using iOSClub.Data.DataModels;
using iOSClub.DataApi.CQRS.Commands;
using iOSClub.DataApi.Repositories;

namespace iOSClub.DataApi.CQRS.Handlers;

public class ArticleCommandHandler(IArticleRepository articleRepository) : 
    ICommandHandler<CreateArticleCommand, bool>,
    ICommandHandler<UpdateArticleCommand, bool>,
    ICommandHandler<DeleteArticleCommand, bool>,
    ICommandHandler<UpdateManyArticlesCommand, bool>
{
    public async Task<bool> Handle(CreateArticleCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法创建或更新文章
        var result = await articleRepository.CreateOrUpdate(command.Article);
        return result;
    }

    public async Task<bool> Handle(UpdateArticleCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法更新文章
        var result = await articleRepository.CreateOrUpdate(command.Article);
        return result;
    }

    public async Task<bool> Handle(DeleteArticleCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法删除文章
        var result = await articleRepository.Delete(command.Id);
        return result;
    }

    public async Task<bool> Handle(UpdateManyArticlesCommand command, CancellationToken cancellationToken = default)
    {
        // 调用仓库方法批量更新文章
        var result = await articleRepository.UpdateArticleOrders(
            command.Articles.ToDictionary(a => a.Path, a => 0)); // 使用临时顺序值
        return result;
    }
}