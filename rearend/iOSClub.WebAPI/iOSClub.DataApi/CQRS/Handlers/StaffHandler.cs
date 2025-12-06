using iOSClub.Data.DataModels;
using iOSClub.Data.ShowModels;
using iOSClub.DataApi.CQRS.Commands;
using iOSClub.DataApi.CQRS.Queries;
using iOSClub.DataApi.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace iOSClub.DataApi.CQRS.Handlers;

public class StaffQueryHandler(IStaffRepository staffRepository, IDistributedCache distributedCache) : 
    IQueryHandler<GetStaffsQuery, IEnumerable<StaffModel>>,
    IQueryHandler<GetStaffsAsMembersQuery, IEnumerable<MemberModel>>,
    IQueryHandler<GetStaffByIdQuery, StaffModel?>,
    IQueryHandler<GetStaffByIdWithoutOtherDataQuery, StaffModel?>,
    IQueryHandler<GetStaffsByIdentitiesQuery, IEnumerable<StaffModel>>
{
    private const string StaffsCacheKey = "staffs:all";
    private const string StaffsAsMembersCacheKey = "staffs:members";
    private const string StaffCacheKeyPrefix = "staffs:";
    private const string StaffWithoutOtherDataCacheKeyPrefix = "staffs:withoutdata:";
    private const string StaffsByIdentitiesCacheKeyPrefix = "staffs:identities:";
    private const int CacheExpirationMinutes = 30;

    public async Task<IEnumerable<StaffModel>> Handle(GetStaffsQuery query, CancellationToken cancellationToken = default)
    {
        // 尝试从缓存获取
        var cachedStaffs = await distributedCache.GetStringAsync(StaffsCacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedStaffs))
        {
            return JsonConvert.DeserializeObject<IEnumerable<StaffModel>>(cachedStaffs)!;
        }

        // 缓存不存在，从数据库获取
        var staffs = await staffRepository.GetAllStaffAsync();

        // 存入缓存
        await distributedCache.SetStringAsync(
            StaffsCacheKey,
            JsonConvert.SerializeObject(staffs),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
            cancellationToken);

        return staffs;
    }

    public async Task<IEnumerable<MemberModel>> Handle(GetStaffsAsMembersQuery query, CancellationToken cancellationToken = default)
    {
        // 尝试从缓存获取
        var cachedMembers = await distributedCache.GetStringAsync(StaffsAsMembersCacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedMembers))
        {
            return JsonConvert.DeserializeObject<IEnumerable<MemberModel>>(cachedMembers)!;
        }

        // 缓存不存在，从数据库获取
        var members = await staffRepository.GetAllStaffIdentity();

        // 存入缓存
        await distributedCache.SetStringAsync(
            StaffsAsMembersCacheKey,
            JsonConvert.SerializeObject(members),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
            cancellationToken);

        return members;
    }

    public async Task<StaffModel?> Handle(GetStaffByIdQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{StaffCacheKeyPrefix}{query.UserId}";
        
        // 尝试从缓存获取
        var cachedStaff = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedStaff))
        {
            return JsonConvert.DeserializeObject<StaffModel>(cachedStaff);
        }

        // 缓存不存在，从数据库获取
        var staff = await staffRepository.GetStaffByIdAsync(query.UserId);

        if (staff != null)
        {
            // 存入缓存
            await distributedCache.SetStringAsync(
                cacheKey,
                JsonConvert.SerializeObject(staff),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                cancellationToken);
        }

        return staff;
    }

    public async Task<StaffModel?> Handle(GetStaffByIdWithoutOtherDataQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{StaffWithoutOtherDataCacheKeyPrefix}{query.UserId}";
        
        // 尝试从缓存获取
        var cachedStaff = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedStaff))
        {
            return JsonConvert.DeserializeObject<StaffModel>(cachedStaff);
        }

        // 缓存不存在，从数据库获取
        var staff = await staffRepository.GetStaffByIdWithoutOtherData(query.UserId);

        if (staff != null)
        {
            // 存入缓存
            await distributedCache.SetStringAsync(
                cacheKey,
                JsonConvert.SerializeObject(staff),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
                cancellationToken);
        }

        return staff;
    }

    public async Task<IEnumerable<StaffModel>> Handle(GetStaffsByIdentitiesQuery query, CancellationToken cancellationToken = default)
    {
        // 创建一个唯一的缓存键，基于身份列表
        var identitiesKey = string.Join(",", query.Identities.OrderBy(i => i));
        var cacheKey = $"{StaffsByIdentitiesCacheKeyPrefix}{identitiesKey}";
        
        // 尝试从缓存获取
        var cachedStaffs = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedStaffs))
        {
            return JsonConvert.DeserializeObject<IEnumerable<StaffModel>>(cachedStaffs)!;
        }

        // 缓存不存在，从数据库获取
        var staffs = await staffRepository.GetStaffsByIdentitiesAsync(query.Identities);

        // 存入缓存
        await distributedCache.SetStringAsync(
            cacheKey,
            JsonConvert.SerializeObject(staffs),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) },
            cancellationToken);

        return staffs;
    }
}

public class StaffCommandHandler(IStaffRepository staffRepository, IDistributedCache distributedCache) : 
    ICommandHandler<CreateStaffCommand, bool>,
    ICommandHandler<UpdateStaffCommand, bool>,
    ICommandHandler<DeleteStaffCommand, bool>,
    ICommandHandler<ChangeStaffDepartmentCommand, bool>
{
    private const string StaffsCacheKey = "staffs:all";
    private const string StaffsAsMembersCacheKey = "staffs:members";
    private const string StaffCacheKeyPrefix = "staffs:";
    private const string StaffWithoutOtherDataCacheKeyPrefix = "staffs:withoutdata:";
    private const string StaffsByIdentitiesCacheKeyPrefix = "staffs:identities:";

    public async Task<bool> Handle(CreateStaffCommand command, CancellationToken cancellationToken = default)
    {
        var result = await staffRepository.CreateStaffAsync(command.Staff);
        
        if (result)
        {
            // 清除相关缓存
            await ClearStaffCache(command.Staff.UserId, cancellationToken);
        }
        
        return result;
    }

    public async Task<bool> Handle(UpdateStaffCommand command, CancellationToken cancellationToken = default)
    {
        var result = await staffRepository.UpdateStaffAsync(command.Staff);
        
        if (result)
        {
            // 清除相关缓存
            await ClearStaffCache(command.Staff.UserId, cancellationToken);
        }
        
        return result;
    }

    public async Task<bool> Handle(DeleteStaffCommand command, CancellationToken cancellationToken = default)
    {
        var result = await staffRepository.DeleteStaffAsync(command.UserId);
        
        if (result)
        {
            // 清除相关缓存
            await ClearStaffCache(command.UserId, cancellationToken);
        }
        
        return result;
    }

    public async Task<bool> Handle(ChangeStaffDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        var result = await staffRepository.ChangeStaffDepartmentAsync(command.UserId, command.DepartmentName);
        
        if (result)
        {
            // 清除相关缓存
            await ClearStaffCache(command.UserId, cancellationToken);
        }
        
        return result;
    }

    // 清除员工相关缓存的辅助方法
    private async Task ClearStaffCache(string userId, CancellationToken cancellationToken)
    {
        // 清除所有员工缓存
        await distributedCache.RemoveAsync(StaffsCacheKey, cancellationToken);
        await distributedCache.RemoveAsync(StaffsAsMembersCacheKey, cancellationToken);
        
        // 清除单个员工缓存
        await distributedCache.RemoveAsync($"{StaffCacheKeyPrefix}{userId}", cancellationToken);
        await distributedCache.RemoveAsync($"{StaffWithoutOtherDataCacheKeyPrefix}{userId}", cancellationToken);
        
        // 注意：在实际生产环境中，可能需要更精细地清除按身份查询的缓存
        // 这里为了简单起见，只清除了主要缓存
    }
}