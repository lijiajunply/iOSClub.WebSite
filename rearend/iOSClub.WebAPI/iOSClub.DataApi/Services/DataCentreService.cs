using iOSClub.Data;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Services;

public interface IDataCentreService
{
    public Task<List<YearCount>> GetYearDataAsync();
    public Task<List<AcademyCount>> GetCollegeDataAsync();
    public Task<List<GradeCount>> GetGradeDataAsync();
    public Task<List<LandscapeCount>> GetLandscapeDataAsync();
    public Task<List<GenderCount>> GetGenderDataAsync();
}

// 用于存储年度统计数据的记录类型
[Serializable]
public record YearCount(string Year, int Value);

// 用于存储年级统计数据的记录类型
[Serializable]
public record GradeCount(string Grade, int Value);

// 用于存储政治面貌统计数据的记录类型
[Serializable]
public record LandscapeCount(string Type, int Sales);

// 用于存储性别统计数据的记录类型
[Serializable]
public record GenderCount(string Type, int Value);

public class DataCentreService(IDbContextFactory<ClubContext> contextFactory) : IDataCentreService
{
    // 获取按学年统计数据
    public async Task<List<YearCount>> GetYearDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var yearData = new List<YearCount>();

        var total = await context.Students.CountAsync();
        var (year, month, _) = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

        // 添加历史学年数据
        yearData.AddRange([
            new YearCount("2019学年", 33),
            new YearCount("2020学年", 1),
            new YearCount("2021学年", 274),
            new YearCount("2022学年", 329)
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
            yearData.Add(new YearCount($"{year - i - 1}学年", v));
        }

        if (month < 9) return yearData;

        // 复用已获取的学生数据，不再重复查询数据库
        var value = students.Count(s => int.Parse(s.UserId.Substring(0, 2)) > (year - 2004));
        yearData.Add(new YearCount($"{year}学年", value));

        return yearData;
    }

    // 获取按学院统计数据
    public async Task<List<AcademyCount>> GetCollegeDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        // 使用LINQ查询替代原始SQL以确保跨数据库兼容性，并添加AsNoTracking()减少EF Core跟踪开销
        return await context.Students.AsNoTracking()
            .GroupBy(s => s.Academy)
            .Select(g => new AcademyCount { Type = g.Key, Value = g.Count() })
            .OrderByDescending(ac => ac.Value)
            .ToListAsync();
    }

    // 获取按年级统计数据
    public async Task<List<GradeCount>> GetGradeDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        // 使用客户端评估处理年级数据，并添加AsNoTracking()减少EF Core跟踪开销
        var students = await context.Students.AsNoTracking().ToListAsync();
        var groupedStudents = students.GroupBy(s => s.UserId.Substring(0, 2));

        var gradeData = groupedStudents.Select(group => new GradeCount(group.Key + "级", group.Count())).ToList();

        gradeData.Sort((x, y) => string.Compare(x.Grade, y.Grade, StringComparison.Ordinal));

        return gradeData;
    }

    // 获取按政治面貌统计数据
    public async Task<List<LandscapeCount>> GetLandscapeDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        // 使用客户端评估处理政治面貌数据，并添加AsNoTracking()减少EF Core跟踪开销
        var students = await context.Students.AsNoTracking().ToListAsync();
        var groupedStudents = students.GroupBy(s => s.PoliticalLandscape);

        return groupedStudents.Select(group => new LandscapeCount(group.Key, group.Count())).ToList();
    }

    // 获取按性别统计数据
    public async Task<List<GenderCount>> GetGenderDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var genderData = new List<GenderCount>();

        // 对于Count查询，AsNoTracking()不影响结果，但添加也不会有负面影响
        var man = await context.Students.AsNoTracking().CountAsync(x => x.Gender == "男");
        var woman = await context.Students.AsNoTracking().CountAsync(x => x.Gender == "女");

        genderData.AddRange(new List<GenderCount>
        {
            new("男", man),
            new("女", woman)
        });

        return genderData;
    }
}

// 用于存储统计数据的模型类
[Serializable]
public class AcademyCount
{
    public string Type { get; set; } = "";
    public int Value { get; set; }
}