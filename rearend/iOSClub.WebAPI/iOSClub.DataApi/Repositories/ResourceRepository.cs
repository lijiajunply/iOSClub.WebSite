using iOSClub.Data;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

public interface IResourceRepository
{
    Task<List<ResourceModel>> GetAllResourcesAsync();
    Task<ResourceModel?> GetResourceByIdAsync(string id);
    Task<List<ResourceModel>> GetResourcesByTagAsync(string tag);
    Task<List<ResourceModel>> SearchResourcesByNameAsync(string name);
    Task<bool> AddResourceAsync(ResourceModel resource);
    Task<bool> UpdateResourceAsync(ResourceModel resource);
    Task<bool> DeleteResourceAsync(string id);
}

public class ResourceRepository(iOSContext context) : IResourceRepository
{
    private readonly iOSContext _context = context;

    /// <summary>
    /// 获取所有资源
    /// </summary>
    public async Task<List<ResourceModel>> GetAllResourcesAsync()
    {
        return await _context.Resources.ToListAsync();
    }

    /// <summary>
    /// 根据ID获取资源
    /// </summary>
    public async Task<ResourceModel?> GetResourceByIdAsync(string id)
    {
        return await _context.Resources.FindAsync(id);
    }

    /// <summary>
    /// 根据标签获取资源
    /// </summary>
    public async Task<List<ResourceModel>> GetResourcesByTagAsync(string tag)
    {
        return await _context.Resources
            .Where(r => r.Tag != null && r.Tag.Contains(tag))
            .ToListAsync();
    }

    /// <summary>
    /// 根据名称搜索资源
    /// </summary>
    public async Task<List<ResourceModel>> SearchResourcesByNameAsync(string name)
    {
        return await _context.Resources
            .Where(r => r.Name.Contains(name))
            .ToListAsync();
    }

    /// <summary>
    /// 添加资源
    /// </summary>
    public async Task<bool> AddResourceAsync(ResourceModel resource)
    {
        try
        {
            // 生成唯一ID
            resource.Id = Guid.NewGuid().ToString("N");
            await _context.Resources.AddAsync(resource);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 更新资源
    /// </summary>
    public async Task<bool> UpdateResourceAsync(ResourceModel resource)
    {
        try
        {
            _context.Resources.Update(resource);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 删除资源
    /// </summary>
    public async Task<bool> DeleteResourceAsync(string id)
    {
        try
        {
            var resource = await GetResourceByIdAsync(id);
            if (resource == null)
                return false;

            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查资源是否存在
    /// </summary>
    public async Task<bool> ResourceExistsAsync(string id)
    {
        return await _context.Resources.AnyAsync(r => r.Id == id);
    }

    /// <summary>
    /// 获取所有标签
    /// </summary>
    public async Task<List<string>> GetAllTagsAsync()
    {
        return await _context.Resources
            .Where(r => r.Tag != null)
            .Select(r => r.Tag!)
            .Distinct()
            .ToListAsync();
    }

    /// <summary>
    /// 获取资源数量
    /// </summary>
    public async Task<int> GetResourceCountAsync()
    {
        return await _context.Resources.CountAsync();
    }
}