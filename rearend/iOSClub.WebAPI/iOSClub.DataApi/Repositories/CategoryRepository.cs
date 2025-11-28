using iOSClub.Data;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

public class CategoryRepository(IDbContextFactory<ClubContext> factory) : ICategoryRepository
{
    public async Task<IEnumerable<CategoryModel>> GetAll()
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Categories
            .OrderBy(c => c.Order)
            .ToListAsync();
    }

    public async Task<CategoryModel?> GetByName(string name)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Categories
            .FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<bool> CreateOrUpdate(CategoryModel model)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        var category = await context.Categories
            .FirstOrDefaultAsync(c => c.Name == model.Name);
        
        if (category == null)
        {
            // 如果是新分类，确保Id唯一
            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = Guid.NewGuid().ToString();
            }
            context.Categories.Add(model);
        }
        else
        {
            // 更新现有分类
            category.Description = model.Description;
            category.Order = model.Order;
        }
        
        return await context.SaveChangesAsync() == 1;
    }

    public async Task<bool> Delete(string name)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        var category = await context.Categories
            .FirstOrDefaultAsync(c => c.Name == name);
        
        if (category == null)
        {
            return false;
        }
        
        context.Categories.Remove(category);
        return await context.SaveChangesAsync() == 1;
    }

    public async Task<bool> UpdateCategoryOrder(string name, int order)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        var category = await context.Categories
            .FirstOrDefaultAsync(c => c.Name == name);
        
        if (category == null)
        {
            return false;
        }
        
        category.Order = order;
        return await context.SaveChangesAsync() == 1;
    }

    public async Task<bool> UpdateCategoryOrders(Dictionary<string, int> categoryOrders)
    {
        if (categoryOrders == null || categoryOrders.Count == 0)
        {
            return true;
        }

        await using var context = await factory.CreateDbContextAsync();
        
        // 开始事务
        await using var transaction = await context.Database.BeginTransactionAsync();
        
        try
        {
            // 获取所有需要更新的分类 - 使用ID而不是名称
            var categoryIds = categoryOrders.Keys.ToList();
            var categories = await context.Categories
                .Where(c => categoryIds.Contains(c.Id))
                .ToListAsync();
            
            // 更新每个分类的顺序
            foreach (var category in categories)
            {
                if (categoryOrders.TryGetValue(category.Id, out var order))
                {
                    category.Order = order;
                }
            }
            
            // 保存更改
            var result = await context.SaveChangesAsync();
            
            // 提交事务
            await transaction.CommitAsync();
            
            // 返回是否所有请求的分类都被更新
            return result == categories.Count;
        }
        catch
        {
            // 回滚事务
            await transaction.RollbackAsync();
            throw;
        }
    }
}