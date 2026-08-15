using iOSClub.Data;
using iOSClub.Data.VOs;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Services;

/// <summary>
/// 数据中心服务接口，提供各种统计数据的查询功能
/// </summary>
public interface IDataCentreService
{
    /// <summary>
    /// 获取按学年统计的数据
    /// </summary>
    /// <returns>学年统计数据列表</returns>
    public Task<List<YearCountVO>> GetYearDataAsync();

    /// <summary>
    /// 获取按学院统计的数据
    /// </summary>
    /// <returns>学院统计数据列表</returns>
    public Task<List<AcademyCountVO>> GetCollegeDataAsync();

    /// <summary>
    /// 获取按年级统计的数据
    /// </summary>
    /// <returns>年级统计数据列表</returns>
    public Task<List<GradeCountVO>> GetGradeDataAsync();

    /// <summary>
    /// 获取按政治面貌统计的数据
    /// </summary>
    /// <returns>政治面貌统计数据列表</returns>
    public Task<List<LandscapeCountVO>> GetLandscapeDataAsync();

    /// <summary>
    /// 获取按性别统计的数据
    /// </summary>
    /// <returns>性别统计数据列表</returns>
    public Task<List<GenderCountVO>> GetGenderDataAsync();
}

public class DataCentreService(IDbContextFactory<ClubContext> contextFactory) : IDataCentreService
{
    // 获取按学年统计数据
    public async Task<List<YearCountVO>> GetYearDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var yearData = new List<YearCountVO>();

        var total = await context.Students.CountAsync();
        var (year, month, _) = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

        // 添加历史学年数据
        yearData.AddRange([
            new YearCountVO { Year = "2019学年", Value = 33 },
            new YearCountVO { Year = "2020学年", Value = 1 },
            new YearCountVO { Year = "2021学年", Value = 274 },
            new YearCountVO { Year = "2022学年", Value = 329 }
        ]);

        if (total <= 430) return yearData;

        // 一次性获取所有学生数据，并添加AsNoTracking()减少EF Core跟踪开销
        var students = await context.Students.AsNoTracking().ToListAsync();

        for (var i = year - 2024; i >= 0; i--)
        {
            var date = new DateTime(year - i, 9, 1, 0, 0, 0, DateTimeKind.Utc);
            var a = year - i - 2005;

            // 使用客户端评估来处理字符串到整数的转换
            var v = students.Count(s => s.JoinTime < date && int.Parse(s.UserId[..2]) > a);
            yearData.Add(new YearCountVO { Year = $"{year - i - 1}学年", Value = v });
        }

        if (month < 9) return yearData;

        // 复用已获取的学生数据，不再重复查询数据库
        var value = students.Count(s => int.Parse(s.UserId[..2]) > year - 2004);
        yearData.Add(new YearCountVO { Year = $"{year}学年", Value = value });

        return yearData;
    }

    // 获取按学院统计数据
    public async Task<List<AcademyCountVO>> GetCollegeDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        // 使用LINQ查询替代原始SQL以确保跨数据库兼容性，并添加AsNoTracking()减少EF Core跟踪开销
        return await context.Students.AsNoTracking()
            .GroupBy(s => s.Academy)
            .Select(g => new AcademyCountVO { Type = g.Key, Value = g.Count() })
            .OrderByDescending(ac => ac.Value)
            .ToListAsync();
    }

    // 获取按年级统计数据
    public async Task<List<GradeCountVO>> GetGradeDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        // 使用客户端评估处理年级数据，并添加AsNoTracking()减少EF Core跟踪开销
        var students = await context.Students.AsNoTracking().ToListAsync();
        var groupedStudents = students.GroupBy(s => s.UserId.Substring(0, 2));

        var gradeData = groupedStudents.Select(group => new GradeCountVO { Grade = group.Key + "级", Value = group.Count() }).ToList();

        gradeData.Sort((x, y) => string.Compare(x.Grade, y.Grade, StringComparison.Ordinal));

        return gradeData;
    }

    // 获取按政治面貌统计数据
    public async Task<List<LandscapeCountVO>> GetLandscapeDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        // 使用客户端评估处理政治面貌数据，并添加AsNoTracking()减少EF Core跟踪开销
        var students = await context.Students.AsNoTracking().ToListAsync();
        var groupedStudents = students.GroupBy(s => s.PoliticalLandscape);

        return groupedStudents.Select(group => new LandscapeCountVO { Type = group.Key, Value = group.Count() }).ToList();
    }

    // 获取按性别统计数据
    public async Task<List<GenderCountVO>> GetGenderDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var genderData = new List<GenderCountVO>();

        // 对于Count查询，AsNoTracking()不影响结果，但添加也不会有负面影响
        var man = await context.Students.AsNoTracking().CountAsync(x => x.Gender == "男");
        var woman = await context.Students.AsNoTracking().CountAsync(x => x.Gender == "女");

        genderData.AddRange(new List<GenderCountVO>
        {
            new GenderCountVO { Type = "男", Value = man },
            new GenderCountVO { Type = "女", Value = woman }
        });

        return genderData;
    }
}