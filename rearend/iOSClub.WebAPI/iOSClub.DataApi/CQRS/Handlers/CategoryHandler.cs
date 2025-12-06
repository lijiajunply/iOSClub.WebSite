using iOSClub.Data.DataModels;
using iOSClub.DataApi.CQRS.Commands;
using iOSClub.DataApi.CQRS.Queries;
using iOSClub.DataApi.Repositories;

namespace iOSClub.DataApi.CQRS.Handlers;

public class CategoryQueryHandler(ICategoryRepository categoryRepository) : 
    IQueryHandler<GetCategoriesQuery, IEnumerable<CategoryModel>>,
    IQueryHandler<GetCategoryByIdQuery, CategoryModel?>,
    IQueryHandler<GetCategoryByNameQuery, CategoryModel?>
{
    public async Task<IEnumerable<CategoryModel>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken = default)
    {
        // 从数据库获取所有分类
        var categories = await categoryRepository.GetAll();
        return categories;
    }

    public async Task<CategoryModel?> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken = default)
    {
        // 从数据库获取指定ID的分类
        var category = await categoryRepository.GetById(query.Id);
        return category;
    }

    public async Task<CategoryModel?> Handle(GetCategoryByNameQuery query, CancellationToken cancellationToken = default)
    {
        // 从数据库获取指定名称的分类
        var category = await categoryRepository.GetByName(query.Name);
        return category;
    }
}

public class CategoryCommandHandler(ICategoryRepository categoryRepository) : 
    ICommandHandler<CreateCategoryCommand, bool>,
    ICommandHandler<UpdateCategoryCommand, bool>,
    ICommandHandler<DeleteCategoryCommand, bool>,
    ICommandHandler<UpdateManyCategoriesCommand, bool>
{
    public async Task<bool> Handle(CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        // 创建或更新分类
        var result = await categoryRepository.CreateOrUpdate(command.Category);
        return result;
    }

    public async Task<bool> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        // 更新分类
        var result = await categoryRepository.CreateOrUpdate(command.Category);
        return result;
    }

    public async Task<bool> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
    {
        // 删除分类
        var result = await categoryRepository.Delete(command.Id);
        return result;
    }

    public async Task<bool> Handle(UpdateManyCategoriesCommand command, CancellationToken cancellationToken = default)
    {
        // 批量更新分类
        var categoryOrders = command.Categories.ToDictionary(c => c.Id, c => c.Order);
        var result = await categoryRepository.UpdateCategoryOrders(categoryOrders);
        return result;
    }
}