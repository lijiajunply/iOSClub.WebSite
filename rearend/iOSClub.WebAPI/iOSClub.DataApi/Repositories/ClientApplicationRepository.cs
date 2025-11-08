using iOSClub.Data;
using iOSClub.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Repositories;

public interface IClientApplicationRepository
{
    /// <summary>
    /// 获取所有客户端应用
    /// </summary>
    /// <returns>客户端应用列表</returns>
    Task<IEnumerable<ClientApplication>> GetAllAsync();

    /// <summary>
    /// 根据客户端ID获取应用
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <returns>客户端应用</returns>
    Task<ClientApplication?> GetByClientIdAsync(string clientId);

    /// <summary>
    /// 创建新的客户端应用
    /// </summary>
    /// <param name="clientApplication">客户端应用</param>
    /// <returns>是否创建成功</returns>
    Task<bool> CreateAsync(ClientApplication clientApplication);

    /// <summary>
    /// 更新客户端应用
    /// </summary>
    /// <param name="clientApplication">客户端应用</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateAsync(ClientApplication clientApplication);

    /// <summary>
    /// 删除客户端应用
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(string clientId);

    /// <summary>
    /// 验证客户端凭据
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="clientSecret">客户端密钥</param>
    /// <returns>客户端应用</returns>
    Task<ClientApplication?> ValidateCredentialsAsync(string clientId, string clientSecret);
}

public class ClientApplicationRepository(IDbContextFactory<ClubContext> contextFactory) : IClientApplicationRepository
{
    public async Task<IEnumerable<ClientApplication>> GetAllAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.ClientApplications.ToListAsync();
    }

    public async Task<ClientApplication?> GetByClientIdAsync(string clientId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.ClientApplications.FirstOrDefaultAsync(c => c.ClientId == clientId);
    }

    public async Task<bool> CreateAsync(ClientApplication clientApplication)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        // 对客户端密钥进行哈希处理后再存储
        clientApplication.ClientSecret = BCrypt.Net.BCrypt.HashPassword(clientApplication.ClientSecret);
        context.ClientApplications.Add(clientApplication);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(ClientApplication clientApplication)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        // 检查客户端密钥是否需要重新哈希（如果提供了新密钥）
        // 如果客户端密钥不是BCrypt哈希格式，则进行哈希处理
        if (!string.IsNullOrEmpty(clientApplication.ClientSecret) &&
            !clientApplication.ClientSecret.StartsWith("$2"))
        {
            clientApplication.ClientSecret = BCrypt.Net.BCrypt.HashPassword(clientApplication.ClientSecret);
        }

        context.ClientApplications.Update(clientApplication);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(string clientId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var clientApplication = await context.ClientApplications.FirstOrDefaultAsync(c => c.ClientId == clientId);
        if (clientApplication == null) return false;

        context.ClientApplications.Remove(clientApplication);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<ClientApplication?> ValidateCredentialsAsync(string clientId, string clientSecret)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var clientApplication = await context.ClientApplications.FirstOrDefaultAsync(c => c.ClientId == clientId);

        if (clientApplication is { IsActive: true }) return clientApplication;
        Console.WriteLine("客户端应用已禁用");
        return null;
    }
}