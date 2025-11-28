using iOSClub.Data.DataModels;

namespace iOSClub.DataApi.Repositories;

public interface ICategoryRepository
{
    public Task<IEnumerable<CategoryModel>> GetAll();
    public Task<CategoryModel?> GetByName(string name);
    public Task<bool> CreateOrUpdate(CategoryModel model);
    public Task<bool> Delete(string name);
    public Task<bool> UpdateCategoryOrder(string name, int order);
    public Task<bool> UpdateCategoryOrders(Dictionary<string, int> categoryOrders);
}