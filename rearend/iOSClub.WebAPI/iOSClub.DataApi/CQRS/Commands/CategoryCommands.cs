using iOSClub.Data.DataModels;

namespace iOSClub.DataApi.CQRS.Commands;

// 命令：创建分类
public record CreateCategoryCommand(CategoryModel Category) : ICommand;

// 命令：更新分类
public record UpdateCategoryCommand(CategoryModel Category) : ICommand;

// 命令：删除分类
public record DeleteCategoryCommand(string Id) : ICommand;

// 命令：批量更新分类
public record UpdateManyCategoriesCommand(List<CategoryModel> Categories) : ICommand;